// NS_ABSORB
using Helpers.Engine;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TGP
{
    namespace Helpers
    {
        public static class Physics_Helper
        {
            #region Raycast Checks
            public static RaycastHit ScreenRaycast(Vector3 screenPos, float dist,
                RaycastArgs thingsToIgnore, float radius = 0, bool debug = false, Camera camera = null)
            {
                RaycastHit[] results = ScreenRaycastAll(screenPos, dist, thingsToIgnore, radius, debug, camera);
                if (results == null)
                    return new RaycastHit();
                if (results.Length == 0)
                    return new RaycastHit();
                return results[0];
            }

            public static RaycastHit ScreenRaycast(Vector3 screenPos, float dist, LayerMask layerMask, Camera camera = null)
            {
                RaycastHit hitInfo = new RaycastHit();

                if (!camera)
                    camera = Camera.main;

                Ray ray = camera.ScreenPointToRay(screenPos);
                // Debug.DrawRay(ray.origin, ray.origin + ray.direction * dist, Color.blue, 5);		

                if (Physics.Raycast(ray, out hitInfo, dist, layerMask))
                    return hitInfo;
                else
                    return new RaycastHit();

            }

            public static RaycastHit[] ScreenRaycastAll(Vector3 screenPos, float dist,
                RaycastArgs thingsToIgnore, float radius = 0, bool debug = false, Camera camera = null)
            {
                RaycastHit[] hitInfo = null;

                if (!camera)
                    camera = Camera.main;

                if (!camera)
                    camera = Utility_Helper.GetComponentInScene<Camera>();
                if (!camera)
                {
                    Debug_Helper.Log(typeof(Physics_Helper), "Add a damn Camera to the scene..!!");
                    return hitInfo;
                }

                Ray ray = camera.ScreenPointToRay(screenPos);
                // Debug.DrawRay(ray.origin, ray.origin + ray.direction * dist, Color.blue, 5);

                return camera.transform.RaycastTowardsAll(ray.direction, dist, thingsToIgnore, radius, debug);
            }

            public static bool HasLineOfSight(this Transform a, Transform b, LayerMask layerMask)
            {
                Vector3 diff = b.position - a.position;
                RaycastHit info = a.RaycastTowards(diff.normalized, diff.magnitude,
                    new RaycastArgs(layerMask, null, null, true), a.lossyScale.x / 2, true);
                // We didn't hit anything on the way
                if (info.IsNull())
                    return true;
                if (info.transform != b.transform)
                    return false;
                return true;
            }

            /// <summary>
            /// Check with hitInfo.IsNull()
            /// </summary>
            public static RaycastHit CheckTowards(this Transform T, Vector3 dir, float dist, LayerMask layerMask, float radius = 0, bool debug = false)
            {
                RaycastHit[] results = T.CheckTowardsAll(dir, dist, layerMask, radius, debug);
                if (results == null)
                    return new RaycastHit();
                if (results.Length == 0)
                    return new RaycastHit();
                return results[0];
            }
            
            /// <summary>
            /// Returns null if no targets
            /// </summary>
            public static RaycastHit[] CheckTowardsAll(this Transform T, Vector3 dir, float dist, LayerMask layerMask, float radius = 0, bool debug = false)
            {
                RaycastHit[] hitInfos = null;

                dir.Normalize();
                Vector3 minDisplacement = dir * (T.lossyScale.z / 2 + 0.01f);
                Vector3 verticalDisplacement = Vector3.zero; // T1.up * T1.lossyScale.y / 2;

                Ray ray = new Ray(T.position + minDisplacement + verticalDisplacement, dir);

                if (debug)
                    Debug.DrawRay(ray.origin, ray.direction * dist, Color.blue, 3);

                if (radius == 0)
                    hitInfos = Physics.RaycastAll(ray, dist, layerMask);
                else
                    hitInfos = Physics.SphereCastAll(ray, radius, dist, layerMask);

                return hitInfos;
            }

            /// <summary>
            /// Check with hitInfo.IsNull()
            /// </summary>
            public static RaycastHit CheckForward(this Transform T, float dist, LayerMask layerMask, float radius = 0, bool debug = false)
            {
                return T.CheckTowards(T.forward, dist, layerMask, radius, debug);
            }

            /// <summary>
            /// Returns null if no targets
            /// </summary>
            public static RaycastHit[] CheckForwardAll(this Transform T, float dist, LayerMask layerMask, float radius = 0, bool debug = false)
            {
                return T.CheckTowardsAll(T.forward, dist, layerMask, radius, debug);
            }

            /// <summary>
            /// foreach (Player enemy in Utility_Helper.GetAllObjectsNear<Player>(aC.target.position, destabilizeRange, player))
            /// </summary>
            public static T[] GetAllObjectsNear<T>(Vector3 position, float radius, LayerMask layerMask, T ignoreMe = null) where T : UnityEngine.Object
            {
                List<T> objects = new List<T>();
                // Find enemies in our viscinity
                foreach (Collider c in Physics.OverlapSphere(position, radius, layerMask))
                {
                    T enemy = c.GetComponent<T>();

                    // If it's not a player, ignore
                    if (!enemy || enemy == ignoreMe) continue;

                    objects.Add(enemy);
                }
                return objects.ToArray();
            }

            /// <summary>
            /// Check with hitInfo.IsNull()
            /// </summary>
            public static RaycastHit RaycastTowards(this Transform T, Vector3 dir, float dist,
                RaycastArgs thingsToIgnore, float radius = 0, bool debug = false)
            {
                RaycastHit[] results = T.RaycastTowardsAll(dir, dist, thingsToIgnore, radius, debug);
                if (results == null)
                    return new RaycastHit();
                if (results.Length == 0)
                    return new RaycastHit();
                return results[0];
            }

            /// <summary>
            /// Returns null if no targets
            /// </summary>
            public static RaycastHit[] RaycastForwardAll(this Transform T, float dist,
                RaycastArgs thingsToIgnore, float radius = 0, bool debug = false)
            {
                return T.RaycastTowardsAll(T.forward, dist, thingsToIgnore, radius, debug);
            }

            /// <summary>
            /// Check with hitInfo.IsNull()
            /// </summary>
            public static RaycastHit RaycastForward(this Transform T, float dist,
                RaycastArgs thingsToIgnore, float radius = 0, bool debug = false)
            {
                return T.RaycastTowards(T.forward, dist, thingsToIgnore, radius, debug);
            }

            /// <summary>
            /// Returns null if no targets
            /// </summary>
            public static RaycastHit[] RaycastTowardsAll(this Transform T, Vector3 dir, float dist,
                RaycastArgs raycastArgs, float radius = 0, bool debug = false)
            {
                // [DEB] Turn this on / off
                bool forceDebug = false;

                if (Debug_Helper.isDebugBuild && forceDebug)
                    debug = true;

                float debugDuration = 3;

                List<RaycastHit> hitInfos = null;

                dir.Normalize();
                // These could be useful in the raycast towards (not All) // T.lossyScale.z / 2
                // As of v5.3.1f1 there is a where SphereCastAll ignore colliders which are close to you.
                // So we need to start the cast behind us (it's not an issue since we will ignore ourselves anyway)
                Vector3 minDisplacement = -dir * (radius + 0.01f);
                Vector3 verticalDisplacement = Vector3.zero; // T1.up * T1.lossyScale.y / 2;

                Ray ray = new Ray();
                switch (raycastArgs.raycastFrom)
                {
                    // Cast directly from the transform 
                    case RaycastArgs.From.Origin:
                        ray.origin = T.position + minDisplacement + verticalDisplacement;
                        ray.direction = dir + dir.normalized * minDisplacement.magnitude;
                        dist -= minDisplacement.magnitude;
                        break;

                    case RaycastArgs.From.Plane:
                        // The ray's origin is the direction's projection on the plane perp to forward of T
                        ray.origin = T.position + Vector3.ProjectOnPlane(dir, T.forward);
                        // And its direction is the direction's projection on the forward of T
                        ray.direction = Vector3.Project(dir, T.forward);
                        break;
                }

                if (debug)
                    Debug_Helper.DrawRay(ray.origin, ray.direction * dist, Color.blue, debugDuration);

                if (radius == 0)
                    hitInfos = Physics.RaycastAll(ray, dist, raycastArgs.layerMask_Solids).ToList();
                else
                    hitInfos = Physics.SphereCastAll(ray, radius, dist, raycastArgs.layerMask_Solids).ToList();

                // Remove everything which is behind us and order the remaining by distance
                // Required to fix the aforementioned bug & for RaycastTowards to just pick up [0]
                hitInfos = hitInfos
                            .Where(h => Vector3.Dot(h.point - T.position, dir) > 0)
                            .OrderBy(h => h.distance).ToList();

                // Debug.Log("Initial\n" + hitInfos.ToReadableString());

                // Ignore trigger colliders
                List<RaycastHit> filteredHitInfos_AfterIgnoreTriggers = new List<RaycastHit>();
                foreach (RaycastHit rH in hitInfos)
                    // If it's not a trigger, it passes
                    // If it's in the triggers layer mask, it passes
                    if (!rH.collider.isTrigger || raycastArgs.layerMask_Triggers.Contains(rH.collider.gameObject.layer))
                        filteredHitInfos_AfterIgnoreTriggers.Add(rH);

                // Debug.Log("After Ignore TRIGGERS\n" + filteredHitInfos_AfterIgnoreTriggers.ToReadableString());

                // Ignore Tags
                List<RaycastHit> filteredHitInfos_AfterIgnoreTags = new List<RaycastHit>();
                List<Collider> collidersToKeep = new List<Collider>(
                    // The triggers to be filtered out have ALREADY been filtered out
                    filteredHitInfos_AfterIgnoreTriggers.GetAllColliders(true).FilterByTags(KeepOrRemove.Remove, raycastArgs.ignoreTags));
                foreach (RaycastHit rH in filteredHitInfos_AfterIgnoreTriggers)
                {
                    if (collidersToKeep.Contains(rH.collider))
                        filteredHitInfos_AfterIgnoreTags.Add(rH);
                }

                // Debug.Log("After Ignore TAGS\n" + filteredHitInfos_AfterIgnoreTags.ToReadableString());

                // Ignore self hits
                List<RaycastHit> filteredHitInfos_AfterIgnoreObjectsAndSelfHits = new List<RaycastHit>();
                foreach (RaycastHit rH in filteredHitInfos_AfterIgnoreTags)
                {
                    bool isClean = true;
                    if (T.GetComponentsInChildren<Collider>().Contains(rH.collider))
                        isClean = false;
                    foreach (GameObject gO in raycastArgs.ignoreObjects)
                        if (gO.GetComponentsInChildren<Collider>().Contains(rH.collider))
                            isClean = false;

                    if (isClean)
                        filteredHitInfos_AfterIgnoreObjectsAndSelfHits.Add(rH);
                }

                // Debug.Log("(Final) After Ignore OBJECTS\n" + filteredHitInfos_AfterIgnoreObjectsAndSelfHits.ToReadableString());

                if (debug)
                    Debug_Helper.Log(typeof(Utility_Helper),
                        "All:\n{0}\nWithouts Triggers (if selected):\n{1}\nWithouts Ignore Hits:\n{2}\nWithouts Ignore Objects & Self Hits:\n{3}\n"._Format(
                        hitInfos.ToReadableString(), filteredHitInfos_AfterIgnoreTriggers.ToReadableString(),
                        filteredHitInfos_AfterIgnoreTags.ToReadableString(), filteredHitInfos_AfterIgnoreObjectsAndSelfHits.ToReadableString()));

#if DEBUG_EXTENSIONS
        if (debug)
            foreach (RaycastHit rH in hitInfos)
                DebugExtension.DebugPoint(rH.point, Color.red, 0.1f, 3);
#endif

                if (debug)
                    foreach (RaycastHit rH in filteredHitInfos_AfterIgnoreObjectsAndSelfHits)
                        Debug_Helper.DrawPoint(rH.point, 0.1f, Color.red, debugDuration);

                return filteredHitInfos_AfterIgnoreObjectsAndSelfHits.ToArray();
            }


            public static bool IsNull(this RaycastHit hitInfo)
            {
                // Debug.Log(hitInfo.collider);
                return hitInfo.collider == null;
            }
            #endregion


            #region Rigidbody
            public static void PauseRigidbodies<T>(this T obj) where T : Component
            {
                foreach (Rigidbody childRb in obj.GetComponentsInChildren<Rigidbody>())
                    if (childRb != obj)
                        childRb.PauseRigidbodies();
            }

            public static void Pause(this Rigidbody rB)
            {
                rB.velocity = Vector3.zero;
                rB.angularVelocity = Vector3.zero;
            }
            #endregion


            #region Velocity
            public static Vector3 TryGetVelocity(this Transform t)
            {
                Rigidbody r = t.GetComponent<Rigidbody>();
                if (!r)
                    return Vector3.zero;

                return r.velocity;
            }

            public static Vector3 GetRelativeLinearVelocity(this Rigidbody r)
            {
                return r.GetRelativeLinearVelocity(Vector3.zero);
            }

            public static Vector3 GetRelativeLinearVelocity(this Rigidbody r, Vector3 groundVelocity)
            {
                if (!r)
                    return Vector3.zero;
                return r.transform.InverseTransformDirection(r.velocity - groundVelocity);
            }

            public static Vector3 GetRelativeAngularVelocity(this Rigidbody r)
            {
                return r.GetRelativeAngularVelocity(Vector3.zero);
            }

            public static Vector3 GetRelativeAngularVelocity(this Rigidbody r, Vector3 groundAngularVelocity)
            {
                if (!r)
                    return Vector3.zero;
                return r.transform.InverseTransformDirection(r.angularVelocity - groundAngularVelocity);
            }

            // TODO: Add friction Invariance (for now disabling friction works)
            public static Vector3 CalculateInvariantDV(this Vector3 wantedVelocity, Vector3 currentVelocity, float control, float maxSpeedNegationPerSec, float drag, Vector3 constantForces)
            {
                Vector3 dV = Vector3.zero;
                Vector3 velGain = constantForces * TimeWrapper.fixedDeltaTime_NotTS;

                Vector3 goodVelocity, badVelocity;

                // Our starting Point
                dV = wantedVelocity;

                // Cancel out the drag we're going to experience
                dV /= Mathf.Clamp01(1 - drag * TimeWrapper.fixedDeltaTime_NotTS);

                // Cancel out our current speed
                dV -= currentVelocity;

                // Cancel out gravity
                dV -= velGain;

                // Apply drag to the extra part of our velocity
                currentVelocity.SplitVector3GoodBad(wantedVelocity, out goodVelocity, out badVelocity);
                Vector3 extraVelocity = wantedVelocity.normalized * Mathf.Max(goodVelocity.magnitude - wantedVelocity.magnitude, 0);
                dV += extraVelocity;
                dV += badVelocity * (1 - control);

                // And of our gravity
                velGain.SplitVector3GoodBad(wantedVelocity, out goodVelocity, out badVelocity);
                Vector3 extraVelGain = wantedVelocity.normalized * Mathf.Max(goodVelocity.magnitude - wantedVelocity.magnitude - extraVelocity.magnitude, 0);
                dV += extraVelGain;
                dV += badVelocity * (1 - control);

                return dV;
            }
            #endregion
        }

        public struct RaycastArgs
        {
            /// <summary>
            /// Factor those IN when raycasting
            /// </summary>
            public LayerMask layerMask_Solids;

            /// <summary>
            /// Factor those IN when raycasting
            /// </summary>
            public LayerMask layerMask_Triggers;
            public string[] ignoreTags;
            public GameObject[] ignoreObjects;
            public From raycastFrom;

            public static readonly RaycastArgs Default = new RaycastArgs(0);

            public RaycastArgs(int a)
            {
                layerMask_Solids = -1;
                // Don't ignore triggers by default
                layerMask_Triggers = -1;
                ignoreTags = new string[] { };
                ignoreObjects = new GameObject[] { };
                raycastFrom = From.Origin;
            }

            /// <summary>
            /// Null any array you dont want to use
            /// </summary>
            public RaycastArgs(LayerMask layerMask_Solids, string[] ignoreTags, GameObject[] ignoreObjects) : this(0)
            {
                this.layerMask_Solids = layerMask_Solids;

                if (ignoreTags != null)
                    this.ignoreTags = ignoreTags;

                if (ignoreObjects != null)
                    this.ignoreObjects = ignoreObjects;
            }
            /// <summary>
            /// Null any array you dont want to use
            /// </summary>
            public RaycastArgs(LayerMask layerMask_Solids, string[] ignoreTags, GameObject[] ignoreObjects, LayerMask layerMask_Triggers) : this(layerMask_Solids, ignoreTags, ignoreObjects)
            {
                this.layerMask_Triggers = layerMask_Triggers;
            }

            /// <summary>
            /// Null any array you dont want to use
            /// </summary>
            public RaycastArgs(LayerMask layerMask_Solids, string[] ignoreTags, GameObject[] ignoreObjects, bool ignoreAllTriggers) : this(layerMask_Solids, ignoreTags, ignoreObjects)
            {
                if (ignoreAllTriggers)
                    this.layerMask_Triggers = LayerMask.GetMask();
                else
                    layerMask_Triggers = layerMask_Solids;
            }

            public RaycastArgs(LayerMask layerMask, string[] ignoreTags, GameObject[] ignoreObjects, bool ignoreTriggers, From raycastFrom) :
                this(layerMask, ignoreTags, ignoreObjects, ignoreTriggers)
            {
                this.raycastFrom = raycastFrom;
            }

            public RaycastArgs(LayerMask layerMask, string[] ignoreTags, GameObject[] ignoreObjects, LayerMask layerMask_Triggers, From raycastFrom) :
                this(layerMask, ignoreTags, ignoreObjects, layerMask_Triggers)
            {
                this.raycastFrom = raycastFrom;
            }


            // From layermask
            public static implicit operator RaycastArgs(LayerMask obj)
            {
                return new RaycastArgs(obj, null, null, false);
            }

            public enum From { Origin, Plane }
        }

        public enum VelocityType { Linear, Angular }
    }
}
