﻿using System.Collections.Generic;
using MMR.Randomizer.GameObjects;
using MMR.Randomizer.Attributes.Actor;
using MMR.Common.Extensions;
using MMR.Randomizer.Utils;
using System;
using MMR.Randomizer.Attributes;
using System.Linq;

namespace MMR.Randomizer.Extensions
{
    public static class ActorExtensions
    {
        public static int FileListIndex(this Actor actor)
        {
            return actor.GetAttribute<FileIDAttribute>().ID;
        }

        public static int ObjectIndex(this Actor actor)
        {
            return actor.GetAttribute<ObjectListIndexAttribute>()?.Index ?? -1;
        }

        public static bool IsEnemyRandomized(this Actor actor)
        {
            return actor.GetAttribute<EnemizerEnabledAttribute>()?.Enabled ?? false;
        }

        public static bool IsActorRandomized(this Actor actor)
        {
            return actor.GetAttribute<ActorizerEnabledAttribute>()?.Enabled ?? false;
        }

        public static List<int> UnkillableVariants(this Actor actor)
        {
            if (actor.GetAttribute<UnkillableAllVariantsAttribute>() != null) // all are unkillable
            {
                return AllVariants(actor);
            }
            else
            {
                return actor.GetAttribute<UnkillableVariantsAttribute>()?.Variants;
            }
        }

        public static List<int> RespawningVariants(this Actor actor)
        {
            if (actor.GetAttribute<RespawningAllVariantsAttribute>() != null) // all are respawning
            {
                return AllVariants(actor);
            }
            else
            {
                return actor.GetAttribute<RespawningVariantsAttribute>()?.Variants;
            }
        }


        public static List<int> AllVariants(this Actor actor)
        {
            var variants = new List<int>();
            var attrF = actor.GetAttribute<FlyingVariantsAttribute>();
            if (attrF != null)
            {
                variants.AddRange(attrF.Variants);
            }
            var attrW = actor.GetAttribute<WaterVariantsAttribute>();
            if (attrW != null)
            {
                variants.AddRange(attrW.Variants);
            }
            var attrG = actor.GetAttribute<GroundVariantsAttribute>();
            if (attrG != null)
            {
                variants.AddRange(attrG.Variants);
            }
            var attrWall = actor.GetAttribute<WallVariantsAttribute>();
            if (attrWall != null)
            {
                variants.AddRange(attrWall.Variants);
            }
            var attrPatrol = actor.GetAttribute<PatrolVariantsAttribute>();
            if (attrPatrol != null)
            {
                variants.AddRange(attrPatrol.Variants);
            }

            return variants;
        }

        public static List<int> KillableVariants(this Actor actor, List<int> acceptableVariants = null)
        {
            var killableVariants = acceptableVariants != null ? acceptableVariants : AllVariants(actor);
            var unkillableVariants    = UnkillableVariants(actor);
            var respawningVariants    = RespawningVariants(actor);
            if (unkillableVariants != null && unkillableVariants.Count > 0)
            {
                killableVariants.RemoveAll(u => unkillableVariants.Contains(u));
            }
            if (respawningVariants != null && respawningVariants.Count > 0)
            {
                killableVariants.RemoveAll(u => respawningVariants.Contains(u));
            }
            return killableVariants;
        }

        public static List<Scene> ScenesRandomizationExcluded(this Actor actor)
        {
            return actor.GetAttribute<EnemizerScenesExcludedAttribute>()?.ScenesExcluded ?? new List<Scene>();
        }

        public static Models.Rom.Actor ToActorModel(this Actor actor)
        {
            // turning static actor enum into enemy instance
            return new Models.Rom.Actor()
            {
                Name = (actor).ToString(),
                ActorID = (int)actor,
                ActorEnum = actor,
                ObjectID = actor.ObjectIndex(),
                ObjectSize = ObjUtils.GetObjSize(actor.ObjectIndex()),
                Variants = actor.AllVariants(),
                Rotation = new Models.Vectors.vec16(),
                SceneExclude = actor.ScenesRandomizationExcluded()
            };
        }

        public static List<int> CompatibleVariants(this Actor actor, Actor otherActor, Random rng, int oldActorVariant)
        {
            // with mixed types, typing could be messy, keep it hidden here
            // EG. like like can spawn on the sand (land), but also on the bottom of GBC (water floor)

            // I'm sure theres a cleaner way, but everything I tried C# said no
            var listOfVariants = new List<byte>() {1, 2, 3, 4}; // 5
            listOfVariants = listOfVariants.OrderBy(u => rng.Next()).ToList(); // random sort in case it has multiple types
            foreach( var variant in listOfVariants)
            {
                ActorVariantsAttribute ourAttr = null;
                ActorVariantsAttribute theirAttr = null;
                if (variant == 1) // water
                {
                    ourAttr = actor.GetAttribute < WaterVariantsAttribute >();
                    theirAttr = otherActor.GetAttribute< WaterVariantsAttribute >();
                }
                if (variant == 2) // ground
                {
                    ourAttr = actor.GetAttribute<GroundVariantsAttribute>();
                    theirAttr = otherActor.GetAttribute<GroundVariantsAttribute>();
                }
                if (variant == 3) // flying
                {
                    ourAttr = actor.GetAttribute<FlyingVariantsAttribute>();
                    theirAttr = otherActor.GetAttribute<FlyingVariantsAttribute>();
                }
                if (variant == 4) // wall
                {
                    ourAttr = actor.GetAttribute<WallVariantsAttribute>();
                    theirAttr = otherActor.GetAttribute<WallVariantsAttribute>();
                }
                /*if (variant == 5) // patrol
                {
                    ourAttr = actor.GetAttribute<PatrolVariantsAttribute>();
                    theirAttr = otherActor.GetAttribute<PatrolVariantsAttribute>();
                }*/

                // small chance of getting flying enemies on ground enemies
                if (variant == 2 && ourAttr == null) // we are land and they are not
                {
                    theirAttr = otherActor.GetAttribute<FlyingVariantsAttribute>();
                    if (theirAttr != null && rng.Next(100) < 10)
                    {
                        return theirAttr.Variants;
                    }
                }

                if (ourAttr != null && theirAttr != null) // both have same type
                {
                    var compatibleVariants = theirAttr.Variants;
                    // our old actor variant was this type
                    if (compatibleVariants.Count > 0 && ourAttr.Variants.Count > 0 
                        && ourAttr.Variants.Contains(oldActorVariant)) // old actor had to actually be this attribute
                    {
                        return compatibleVariants;
                    }
                }
            }

            return null;
        }

        public static bool IsGroundVariant(this Actor actor, int varient)
        {
            var groundAttribute = actor.GetAttribute<GroundVariantsAttribute>();
            if (groundAttribute != null)
            {
                return groundAttribute.Variants.Contains(varient);
            }
            return false;
        }

        public static bool IsWaterVariant(this Actor actor, int varient)
        {
            var groundAttribute = actor.GetAttribute<WaterVariantsAttribute>();
            if (groundAttribute != null)
            {
                return groundAttribute.Variants.Contains(varient);
            }
            return false;
        }

        public static bool isFlyingVariant(this Actor actor, int varient)
        {
            var groundAttribute = actor.GetAttribute<FlyingVariantsAttribute>();
            if (groundAttribute != null)
            {
                return groundAttribute.Variants.Contains(varient);
            }
            return false;
        }

        public static List<Scene> BlockedScenes(this Actor actor)
        {
            var blockedScenesAttribute = actor.GetAttribute<EnemizerScenesPlacementBlock>();
            if (blockedScenesAttribute != null)
            {
                return blockedScenesAttribute.ScenesBlocked;
            }
            return new List<Scene>();
        }

        public static int VariantMaxCountPerRoom(this Actor actor, int queryVariant) 
        {
            if (actor.GetAttribute<OnlyOneActorPerRoom>() != null)
            {
                return 1;
            }

            var limitedVariants = actor.GetAttributes<VariantsWithRoomMax>();
            if (limitedVariants != null)
            {
                foreach (var variant in limitedVariants)
                {
                    if (variant.Variants.Contains(queryVariant))
                    {
                        return variant.RoomMax;
                    }
                }
            }

            return -1; // no restriction
        }

        public static bool HasVariantsWithRoomLimits(this Actor actor)
        {
            return actor.GetAttributes<VariantsWithRoomMax>() != null
                || OnlyOnePerRoom(actor);
        }

        public static bool OnlyOnePerRoom(this Actor actor)
        {
            return actor.GetAttribute<OnlyOneActorPerRoom>() != null;
        }

        public static bool HasOptionalCompanions(this Actor actor)
        {
            return actor.GetAttributes<AlignedCompanionActorAttribute>() != null;
        }

        public static int ActorInitOffset(this Actor actor)
        {
            return actor.GetAttribute<ActorInitVarOffsetAttribute>()?.Offset ?? -1;
        }
    }
}
