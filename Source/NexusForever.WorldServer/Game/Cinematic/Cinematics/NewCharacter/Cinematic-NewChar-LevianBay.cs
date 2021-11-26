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
    public class Cinematic_NewChar_LevianBay : CinematicBase
    {
        public Cinematic_NewChar_LevianBay(Player player)
        {
            Player = player;
            Duration = 10000;
            InitialFlags = 5;
            InitialCancelMode = 2;
            StartTransition = new Transition(0, 1, 2, 1500, 0, 1500);
            EndTransition = new Transition(9000, 0, 0);

            Setup();
        }

        private void Setup()
        {
            SetupActors();
            SetupCamera();
            SetupTexts();

            // Add Screen Effects
            List<IKeyframeAction> ScreenEffects = new List<IKeyframeAction>();
            ScreenEffects.Add(new VisualEffect(38791, Player.Guid));
            Keyframes.Add("ScreenEffects", ScreenEffects);

            List<IKeyframeAction> PlayerVisuals = new List<IKeyframeAction>();
            PlayerVisuals.Add(new ActorVisibility(0, Player.Guid, true));
            Keyframes.Add("PlayerVisuals", PlayerVisuals);
        }

        private void SetupActors()
        {
            Position initialPosition = new Position(new Vector3(-2712.029052734375f, -1309.1015625f, -6127.48681640625f));
            AddActor(new Actor(0, 0, 0, initialPosition), new List<VisualEffect>
            {
            });
        }

        private void SetupCamera()
        {
            Camera mainCam = new Camera(14794, 0, 0, 1f, useRotation: true);
            AddCamera(mainCam);
            mainCam.AddTransition(0, 0, 1500, 0, 1500);
        }

        private void SetupTexts()
        {
            //AddText(578135, 2900, 5700);
            //AddText(578136, 5800, 11000);
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
