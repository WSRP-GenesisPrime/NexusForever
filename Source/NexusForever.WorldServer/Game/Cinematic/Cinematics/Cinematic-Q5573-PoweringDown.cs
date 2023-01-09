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
    public class Cinematic_Q5573_PoweringDown : CinematicBase
    {
        public Cinematic_Q5573_PoweringDown(Player player)
        {
            Player = player;
            Duration = 14500;
            InitialFlags = 7;
            InitialCancelMode = 2;
            StartTransition = new Transition(0, 1, 2, 750, 1000, 1500);
            EndTransition = new Transition(13000, 0, 0);

            Setup();
        }

        public void Setup()
        {
            SetupActors();
            SetupCamera();

            // Add Screen Effects
            List<IKeyframeAction> ScreenEffects = new List<IKeyframeAction>();
            ScreenEffects.Add(new VisualEffect(22508, Player.Guid));
            ScreenEffects.Add(new VisualEffect(22513, Player.Guid));
            ScreenEffects.Add(new VisualEffect(36672, Player.Guid));
            Keyframes.Add("ScreenEffects", ScreenEffects);
        }

        public void SetupActors()
        {
            Position initialPosition = new Position(new Vector3(-7763.1142578125f, -949.2301025390625f, -273.4617919921875f));
            AddActor(new Actor(33458, 6, 0f, initialPosition, activePropId: 1373867), new List<VisualEffect>
                {
                    new VisualEffect(11096),
                    new VisualEffect(11096)
                });
        }

        public void SetupCamera()
        {
            Camera mainCam = new Camera(9879, 0, 0, 1f, useRotation: true);
            AddCamera(mainCam);
            mainCam.AddTransition(0, 0, 1500, 0, 1500);

            Camera cam2 = new Camera(9880, 0, 4600, 1f, useRotation: true);
            AddCamera(cam2);
            cam2.AddTransition(4600, 0, 1500, 0, 1500);
        }

        protected override void Play()
        {
            base.Play();

            Player.Session.EnqueueMessageEncrypted(new ServerCinematicTransitionDurationSet
            {
                Type = 2,
                DurationStart = 1500,
                DurationMid = 3000,
                DurationEnd = 1500
            });

            foreach (IKeyframeAction keyframeAction in Keyframes.Values.SelectMany(i => i))
                keyframeAction.Send(Player.Session);
        }
    }
}
