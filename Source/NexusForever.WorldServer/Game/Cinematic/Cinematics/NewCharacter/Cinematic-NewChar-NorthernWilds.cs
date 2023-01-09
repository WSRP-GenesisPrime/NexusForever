using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Network.Message.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Game.Cinematic.Cinematics.NewCharacter
{
    public class Cinematic_NewChar_NorthernWilds : CinematicBase
    {
        const uint ACTOR_GRANOK_DROP_POD = 13649;

        public Cinematic_NewChar_NorthernWilds(Player player)
        {
            Player = player;
            Duration = 11000;
            InitialFlags = 7;
            InitialCancelMode = 2;
            StartTransition = new Transition(0, 5, 1, 1500, 0, 1500);
            //EndTransition = new Transition(11000, 0, 0);

            Setup();
        }

        private void Setup()
        {
            SetupActors();
            SetupCamera();
            SetupTexts();

            // Add Screen Effects
            List<IKeyframeAction> ScreenEffects = new List<IKeyframeAction>();
            uint dropPodUnitId = Player.GetVisibleCreature<WorldEntity>(ACTOR_GRANOK_DROP_POD).FirstOrDefault()?.Guid ?? 0u;
            if (dropPodUnitId != 0u)
                ScreenEffects.Add(new VisualEffect(7913, dropPodUnitId));
            ScreenEffects.Add(new VisualEffect(8345, Player.Guid));
            ScreenEffects.Add(new VisualEffect(8341, Player.Guid, initialDelay: 950));
            ScreenEffects.Add(new VisualEffect(8344, Player.Guid, initialDelay: 3000));
            ScreenEffects.Add(new VisualEffect(8340, Player.Guid, initialDelay: 5800));
            ScreenEffects.Add(new VisualEffect(8342, Player.Guid, initialDelay: 7000));
            Keyframes.Add("ScreenEffects", ScreenEffects);

            List<IKeyframeAction> PlayerVisuals = new List<IKeyframeAction>();
            PlayerVisuals.Add(new ActorVisibility(0, Player.Guid, true));
            PlayerVisuals.Add(new ActorVisibility(7000, Player.Guid));
            Keyframes.Add("PlayerVisuals", PlayerVisuals);
        }

        private void SetupActors()
        {
            Position initialPosition = new Position(new Vector3(4110.7099609375f, -660.8599853515625f, -5145.47998046875f));
            AddActor(new Actor(13562, 0, 0.33161258697509766f, initialPosition), new List<VisualEffect>
            {
                new VisualEffect(7913),
                new VisualEffect(5705, initialDelay: 950)
            });

            AddActor(new Actor(14125, 0, 0.33161258697509766f, new Position(new Vector3(4110.7099609375f, -658.8698120117188f, -5145.47998046875f)), initialDelay: 950), new List<VisualEffect>
            {
            });
        }

        private void SetupCamera()
        {
            Camera mainCam = new Camera(3997, 0, 0, 1f, useRotation: true);
            AddCamera(mainCam);
            mainCam.AddTransition(0, 0, 1500, 0, 1500);
        }

        private void SetupTexts()
        {
            AddText(578135, 2900, 5700);
            AddText(578136, 5800, 11000);
        }

        protected override void Play()
        {
            base.Play();

            Player.Session.EnqueueMessageEncrypted(new ServerCinematicTransitionDurationSet
            {
                Type = 2,
                DurationStart = 1500,
                DurationMid = 0,
                DurationEnd = 1500
            });

            foreach (IKeyframeAction keyframeAction in Keyframes.Values.SelectMany(i => i))
                keyframeAction.Send(Player.Session);
        }
    }
}
