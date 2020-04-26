using Unity.Entities;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateAfter(typeof(BuildPhysicsWorld))]
[UpdateAfter(typeof(StepPhysicsWorld))]
[UpdateAfter(typeof(ExportPhysicsWorld))]
public class CollisionSystemGroup : ComponentSystemGroup
{
}