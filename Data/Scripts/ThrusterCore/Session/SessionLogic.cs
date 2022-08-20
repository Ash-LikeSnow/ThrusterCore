using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;
using VRage.Utils;
using Sandbox.Game.Entities;
using ObjectBuilders.SafeZone;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.Entity;
using Sandbox.Game.Entities.Cube;
using System.Collections.Generic;
using Sandbox.Game.EntityComponents;
using VRage.Game.ModAPI.Interfaces;
using static ThrusterCore.Draw;

namespace ThrusterCore
{
    public partial class Session
    {

        internal void CompLoop()
        {

            for (int i = 0; i < GridList.Count; i++)
            {
                var gridComp = GridList[i];
                var grid = gridComp.Grid;
                var positionComp = grid.PositionComp;

                var velocity = grid.Physics.LinearVelocity;
                var velX = Vector3.Dot(velocity, positionComp.WorldMatrixRef.Left);
                var velY = Vector3.Dot(velocity, positionComp.WorldMatrixRef.Down);
                var velZ = Vector3.Dot(velocity, positionComp.WorldMatrixRef.Forward);

                var gravity = grid.Physics.Gravity;

                var controlComp = gridComp.ActiveControl;
                var controller = controlComp?.Controller;
                var active = controller != null && !Vector3.IsZero(controller.MoveIndicator);

                var groupProperties = MyGridPhysicalGroupData.GetGroupSharedProperties(grid);
                var com = groupProperties.CoMWorld;

                if (gridComp.CompTick60 == TickMod60)
                {
                    var worldAABB = grid.PositionComp.WorldAABB;
                    gridComp.ClosestPlanet = MyGamePruningStructure.GetClosestPlanet(ref worldAABB);
                }

                if (gridComp.CompTick20 == TickMod20)
                {
                    var density = gridComp.ClosestPlanet?.GetAirDensity(grid.PositionComp.WorldAABB.Center) ?? 0f;
                    gridComp.AirDirty = !MyUtils.IsZero(gridComp.AirDensity - density, 0.001f);
                    if (gridComp.AirDirty) gridComp.AirDensity = density;
                }

                var powerNeeded = 0f;
                var gasForce = Vector3.Zero;
                var elecForce = Vector3.Zero;
                for (byte j = 0; j < 6; j++)
                {
                    var direction = Base6Directions.Directions[j];
                    DirectionData data;
                    if (!gridComp.DirectionMap.TryGetValue(j, out data))
                        continue;

                    var dampen = true;
                    var manual = false;
                    if (gridComp.UnderControl)
                    {
                        var move = 0f;
                        var indicator = controlComp.Mapping[j];
                        switch (indicator)
                        {
                            case 1:
                                move = controller.MoveIndicator.X;
                                break;
                            case 2:
                                move = controller.MoveIndicator.Y;
                                break;
                            case 3:
                                move = controller.MoveIndicator.Z;
                                break;
                        }
                        dampen = MyUtils.IsZero(move);
                        var even = j % 2 == 0;
                        manual = !dampen && (even && move > 0f || !even && move < 0f);
                    }

                    var thisVel = 0f;
                    switch (j)
                    {
                        case 0:
                            thisVel = velZ;
                            break;
                        case 1:
                            thisVel = -velZ;
                            break;
                        case 2:
                            thisVel = velX;
                            break;
                        case 3:
                            thisVel = -velX;
                            break;
                        case 4:
                            thisVel = -velY;
                            break;
                        case 5:
                            thisVel = velY;
                            break;
                    }
                    dampen &= thisVel > 0.00001f;

                    var maxThrust = 0f;
                    var electricThrust = 0f;
                    var gasThrust = 0f;
                    var power = 0f;

                    for (int k = 0; k < data.ThrustComps.Count; k++)
                    {
                        var thrustComp = data.ThrustComps[k];

                        if (!thrustComp.Functional || !thrustComp.Enabled) continue;

                        //if (!thrustComp.Powered) continue; //What does this mean?

                        var thrust = thrustComp.Definition.ForceMagnitude;

                        if (thrustComp.AffectedByPlanet && gridComp.AirDirty)
                        {
                            var def = thrustComp.Definition;
                            var effPercent = (gridComp.AirDensity - def.MinPlanetaryInfluence) * def.InfluenceRangeInv;
                            thrustComp.Effectiveness = effPercent * (def.EffectivenessAtMaxInfluence - def.EffectivenessAtMinInfluence) + def.EffectivenessAtMinInfluence;
                        }

                        thrust *= thrustComp.Effectiveness;

                        if (!thrustComp.Electric)
                        {
                            if (thrustComp.StoredFuel <= 2f)
                                thrustComp.NeedsFuel = true;

                            if (thrustComp.StoredFuel < 1f)
                                thrust *= thrustComp.StoredFuel;

                            gasThrust += thrust;
                        }
                        else
                        {
                            power += thrustComp.Definition.MaxPowerConsumption * thrustComp.Effectiveness;
                            electricThrust += thrust;
                        }

                        maxThrust += thrust;
                    } //Thrusters loop

                    data.MaxThrust = maxThrust;
                    data.ThrustElectric = electricThrust;
                    data.ThrustGas = gasThrust;

                    if (dampen || manual)
                    {
                        powerNeeded += power;

                        switch (j)
                        {
                            case 0:
                                gasForce.Z = gasThrust;
                                elecForce.Z = electricThrust;
                                break;
                            case 1:
                                gasForce.Z = -gasThrust;
                                elecForce.Z = -electricThrust;
                                break;
                            case 2:
                                gasForce.X = gasThrust;
                                elecForce.X = electricThrust;
                                break;
                            case 3:
                                gasForce.X = -gasThrust;
                                elecForce.X = -electricThrust;
                                break;
                            case 4:
                                gasForce.Y = -gasThrust;
                                elecForce.Y = -electricThrust;
                                break;
                            case 5:
                                gasForce.Y = gasThrust;
                                elecForce.Y = electricThrust;
                                break;
                        }
                    }


                } //Directions loop

                //check powerNeeded against sink available
                if (powerNeeded > 0f)
                {
                    var sink = gridComp.ElectricSink;
                    var distrib = gridComp.Distributor;
                    var electricStrength = MathHelper.Clamp(distrib.MaxAvailableResourceByType(_electricity, grid) / powerNeeded, 0f, 1f);
                    elecForce *= electricStrength;
                    gridComp.Power = powerNeeded * electricStrength;
                    sink.Update();
                }

                DrawLine(grid.PositionComp.WorldAABB.Center, velocity, Color.Blue, 0.5f, 100f);

                var force = gasForce + elecForce;
                var orientation = Quaternion.CreateFromRotationMatrix(grid.PositionComp.WorldMatrixRef);
                var final = Vector3.Zero;
                Vector3.Transform(ref force, ref orientation, out final);
                DrawLine(grid.PositionComp.WorldAABB.Center, Vector3.Normalize(final), Color.OrangeRed, 0.2f, 100f);

                if (!active)
                {
                    var accelSqr = final.LengthSquared() / (groupProperties.Mass * groupProperties.Mass * 3600);
                    if (accelSqr > velocity.LengthSquared())
                        final *= velocity.Length() * groupProperties.Mass * 60 / final.Length();
                }

                grid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, final, com, null);

            } //Grids loop

        }

        internal void StartComps()
        {
            try
            {
                _startGrids.ApplyAdditions();
                for (int i = 0; i < _startGrids.Count; i++)
                {
                    var grid = _startGrids[i];

                    if (grid.IsPreview)
                        continue;

                    var gridComp = _gridCompPool.Count > 0 ? _gridCompPool.Pop() : new GridComp();
                    gridComp.Init(grid, this);

                    GridList.Add(gridComp);
                    GridMap[grid] = gridComp;
                    grid.OnClose += OnGridClose;
                }
                _startGrids.ClearImmediate();

                _startBlocks.ApplyAdditions();
                for (int i = 0; i < _startBlocks.Count; i++)
                {
                    var block = _startBlocks[i];

                    if (block?.CubeGrid?.Physics == null || block.CubeGrid.IsPreview)
                        continue;

                    GridComp gridComp;
                    if (!GridMap.TryGetValue(block.CubeGrid, out gridComp))
                        continue;

                    var sorter = block as MyConveyorSorter;
                    if (sorter != null)
                    {
                        var def = DefinitionMap[sorter.BlockDefinition.Id];
                        var comp = new ThrustComp(sorter, def, this);
                        ThrustMap[block.EntityId] = comp;
                        comp.Init();
                        gridComp.FatBlockAdded(sorter);

                        continue;
                    }

                    var controller = block as MyShipController;
                    if (controller != null)
                    {
                        var control = new Control(controller);
                        gridComp.Controllers.TryAdd(controller, control);
                    }

                }
                _startBlocks.ClearImmediate();
            }
            catch (Exception ex)
            {
                Logs.WriteLine($"Exception in StartComps: {ex}");
            }

        }
    }
}
