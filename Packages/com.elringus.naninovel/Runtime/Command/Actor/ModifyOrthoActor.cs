using UnityEngine;

namespace Naninovel.Commands
{
    public abstract class ModifyOrthoActor<TActor, TState, TMeta, TConfig, TManager> : ModifyActor<TActor, TState, TMeta, TConfig, TManager>
        where TActor : class, IActor
        where TState : ActorState<TActor>, new()
        where TMeta : OrthoActorMetadata
        where TConfig : OrthoActorManagerConfiguration<TMeta>, new()
        where TManager : class, IActorManager<TActor, TState, TMeta, TConfig>
    {
        [Doc("Position (relative to the scene borders, in percents) to set for the modified actor. " +
             "Position is described as follows: `0,0` is the bottom left, `50,50` is the center and `100,100` is the top right corner of the scene. " +
             "Use Z-component (third member, eg `,,10`) to move (sort) by depth while in ortho mode.")]
        [ParameterAlias("pos"), VectorContext("X,Y,Z")]
        public DecimalListParameter ScenePosition;

        // Allows using local scene position to set world position of the actor.
        protected override float?[] AssignedPosition => AttemptScenePosition();
        // Allows using scale=x for uniform scaling.
        protected override float?[] AssignedScale => Assigned(Scale) ? AttemptUniformScale() : base.AssignedScale;
        protected virtual ICameraManager CameraManager => Engine.GetServiceOrErr<ICameraManager>();

        private float?[] worldPosition = new float?[3];
        // ReSharper disable once FieldCanBeMadeReadOnly.Local (making this readonly borks Unity serialization)
        private float?[] uniformScale = new float?[3];

        private float?[] AttemptScenePosition ()
        {
            if (!Assigned(ScenePosition) && PosedPosition is null)
                return base.AssignedPosition;

            if (Assigned(ScenePosition))
            {
                worldPosition[0] = ScenePosition.ElementAtOrNull(0) != null ? CameraManager.Configuration.SceneToWorldSpace(new Vector2(ScenePosition[0] / 100f, 0)).x : default(float?);
                worldPosition[1] = ScenePosition.ElementAtOrNull(1) != null ? CameraManager.Configuration.SceneToWorldSpace(new Vector2(0, ScenePosition[1] / 100f)).y : default(float?);
                worldPosition[2] = ScenePosition.ElementAtOrNull(2);
            }
            else worldPosition = PosedPosition;

            return worldPosition;
        }

        private float?[] AttemptUniformScale ()
        {
            var scale = base.AssignedScale;

            if (scale != null && scale.Length == 1 && scale[0].HasValue)
            {
                var scaleX = scale[0].Value;
                uniformScale[0] = scaleX;
                uniformScale[1] = scaleX;
                uniformScale[2] = scaleX;
                return uniformScale;
            }

            return scale;
        }
    }
}
