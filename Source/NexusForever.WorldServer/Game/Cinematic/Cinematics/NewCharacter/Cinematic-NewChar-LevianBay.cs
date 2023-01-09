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
            Duration = 34033;
            InitialFlags = 7;
            InitialCancelMode = 2;
            CinematicId = 28;
            StartTransition = new Transition(0, 1, 2, 1000, 0, 1500);
            EndTransition = new Transition(32533, 0, 0);

            Setup();
        }

        private void Setup()
        {
            SetupActors();
            SetupCamera();
            SetupTexts();

            // Add Screen Effects
            List<IKeyframeAction> ScreenEffects = new List<IKeyframeAction>();
            ScreenEffects.Add(new VisualEffect(30667, Player.Guid));
            ScreenEffects.Add(new VisualEffect(21853, Player.Guid));
            ScreenEffects.Add(new VisualEffect(29743, Player.Guid));
            ScreenEffects.Add(new VisualEffect(27968, Player.Guid));
            ScreenEffects.Add(new VisualEffect(30489, Player.Guid, initialDelay: 4367));
            Keyframes.Add("ScreenEffects", ScreenEffects);

            List<IKeyframeAction> PlayerVisuals = new List<IKeyframeAction>();
            PlayerVisuals.Add(new ActorVisibility(0, Player.Guid, true));
            Keyframes.Add("PlayerVisuals", PlayerVisuals);
        }

        private void SetupActors()
        {
            Position initialPosition = new Position(new Vector3(-3784.26953125f, -988.6632690429688f, -6188.072265625f));
            AddActor(new Actor(50444, 14, 3.1415929794311523f, initialPosition), new List<VisualEffect>
            {
                new VisualEffect(20016),
                new VisualEffect(20016)
            });

            AddActor(new Actor(50441, 14, 3.1415929794311523f, initialPosition), new List<VisualEffect>
            {
                new VisualEffect(20016)
            });

            AddActor(new Actor(50442, 14, 3.1415929794311523f, initialPosition), new List<VisualEffect>
            {
                new VisualEffect(20016)
            });

            AddActor(new Actor(0, 7, -1.134464144706726f, new Position(new Vector3(-3858.369384765625f, -973.4382934570312f, -6048.97216796875f))), new List<VisualEffect>
            {
                new VisualEffect(21598, initialDelay: 26000)
            });
        }

        private void SetupCamera()
        {
            Camera mainCam = new Camera(GetActor(50444), 7, 0, true, 0);
            AddCamera(mainCam);
            mainCam.AddAttach(6333, 8);
            mainCam.AddTransition(6333, 0, 1500, 0, 1500);
            mainCam.AddAttach(23600, 9);
            mainCam.AddTransition(23600, 0, 1500, 0, 1500);
        }

        private void SetupTexts()
        {
            AddText(578464, 4500, 6567);
            AddText(578465, 6633, 10200);
            AddText(578466, 10300, 11400);
            AddText(578467, 11500, 13600);
            AddText(578468, 13700, 18000);
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
