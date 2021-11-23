using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Network.Message.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Game.Cinematic.Cinematics
{
    public class Cinematic_Q5604_TacticalDemolitions : CinematicBase
    {
        public Cinematic_Q5604_TacticalDemolitions(Player player)
        {
            Player = player;
            Duration = 15000;
            InitialFlags = 7;
            InitialCancelMode = 2;
            StartTransition = new Transition(0, 1, 2, 750, 1000, 1500);
            EndTransition = new Transition(13500, 0, 0);

            Setup();
        }

        private void Setup()
        {
            SetupActors();
            SetupCamera();
            SetupTexts();

            // Add Screen Effects
            List<IKeyframeAction> ScreenEffects = new List<IKeyframeAction>();
            ScreenEffects.Add(new VisualEffect(22518, Player.Guid));
            ScreenEffects.Add(new VisualEffect(22517, Player.Guid));
            ScreenEffects.Add(new VisualEffect(30478, Player.Guid, initialDelay: 1300));
            ScreenEffects.Add(new VisualEffect(30480, Player.Guid, initialDelay: 6600));
            ScreenEffects.Add(new VisualEffect(30479, Player.Guid, initialDelay: 10600));
            Keyframes.Add("ScreenEffects", ScreenEffects);
        }

        private void SetupActors()
        {
            Position initialPosition = new Position(new Vector3(-7856.59521484375f, -941.21630859375f, -1357.636962890625f));
            AddActor(new Actor(33566, 6, 3.1415929794311523f, initialPosition), new List<VisualEffect>
                {
                    new VisualEffect(11096)
                });
        }

        private void SetupCamera()
        {
            Camera mainCam = new Camera(9894, 0, 0, 1f, useRotation: true);
            AddCamera(mainCam);
            mainCam.AddTransition(0, 0, 1500, 0, 1500);
        }

        private void SetupTexts()
        {
            AddText(578425, 1400, 3800);
            AddText(578426, 3867, 6500);
            AddText(578427, 6667, 10400);
            AddText(578428, 10500, 13500);
        }

        protected override void Play()
        {
            base.Play();

            Player.Session.EnqueueMessageEncrypted(new ServerCinematicTransitionDurationSet
            {
                Type = 2,
                DurationStart = 1500,
                DurationMid = 1000,
                DurationEnd = 1500
            });

            foreach (IKeyframeAction keyframeAction in Keyframes.Values.SelectMany(i => i))
                keyframeAction.Send(Player.Session);
        }
    }
}
