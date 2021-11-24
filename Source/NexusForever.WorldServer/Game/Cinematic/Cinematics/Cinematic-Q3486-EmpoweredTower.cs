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
    public class Cinematic_Q3486_EmpoweredTower : CinematicBase
    {
        public Cinematic_Q3486_EmpoweredTower(Player player)
        {
            Player = player;
            Duration = 17000;
            InitialFlags = 7;
            InitialCancelMode = 2;
            StartTransition = new Transition(0, 1, 2, 1500, 0, 1500);
            EndTransition = new Transition(15500, 0, 0);

            Setup();
        }

        public void Setup()
        {
            SetupCamera();

            // Add Screen Effects
            List<IKeyframeAction> ScreenEffects = new List<IKeyframeAction>();
            ScreenEffects.Add(new VisualEffect(21853, Player.Guid));
            ScreenEffects.Add(new VisualEffect(7668, Player.Guid));
            Keyframes.Add("ScreenEffects", ScreenEffects);
        }

        public void SetupCamera()
        {
            Camera mainCam = new Camera(3881, 0, 0, 1f, useRotation: true);
            AddCamera(mainCam);
            mainCam.AddTransition(0, 0, 1500, 0, 1500);
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
