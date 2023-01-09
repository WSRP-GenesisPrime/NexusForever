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
    public class Cinematic_NewChar_CrimsonIsle : CinematicBase
    {
        const uint VO_MONDO = 30484;

        public Cinematic_NewChar_CrimsonIsle(Player player)
        {
            Player = player;
            Duration = 18500;
            InitialFlags = 7;
            InitialCancelMode = 2;
            StartTransition = new Transition(0, 5, 1, 1500, 0, 1500);
            EndTransition = new Transition(17000, 0, 0);

            Setup();
        }

        private void Setup()
        {
            SetupActors();
            SetupCamera();
            SetupTexts();

            // Add Screen Effects
            List<IKeyframeAction> ScreenEffects = new List<IKeyframeAction>();
            ScreenEffects.Add(new VisualEffect(VO_MONDO, Player.Guid, initialDelay: 950));
            Keyframes.Add("ScreenEffects", ScreenEffects);
        }

        private void SetupActors()
        {
            // TODO: Need parse of Crimson Isle cinematic to finish
        }

        private void SetupCamera()
        {
            // TODO: Need parse of Crimson Isle cinematic to finish

            Camera mainCam = new Camera(72364, 0, 0, 1f, useRotation: true);
            AddCamera(mainCam);
            mainCam.AddTransition(0, 0, 1500, 0, 1500);
        }

        private void SetupTexts()
        {
            AddText(578443, 1000, 2500);
            AddText(578444, 2600, 8000);
            AddText(578445, 8100, 12600);
            AddText(578446, 12700, 16500);
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
