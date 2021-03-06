﻿using System;
using UnityEngine;
using Verse;

namespace RA
{
    public class MapCompCameraShaker : MapComponent
    {
        public const float ShakeDecayRate = 0.5f;
        public const float ShakeFrequency = 24f;
        public const float MaxShakeMag = 1f;
        public static float curShakeMag;

        public static Vector3 ShakeOffset
        {
            get
            {
                var x = (float)Math.Sin(Time.realtimeSinceStartup * ShakeFrequency) * curShakeMag;
                var y = (float)Math.Sin(Time.realtimeSinceStartup * ShakeFrequency * 1.05) * curShakeMag;
                var z = (float)Math.Sin(Time.realtimeSinceStartup * ShakeFrequency * 1.1) * curShakeMag;
                return new Vector3(x, y, z);
            }
        }

        public static void DoShake(float mag)
        {
            curShakeMag += mag;
        }

        public override void MapComponentTick()
        {
            if (curShakeMag > 0)
            {
                if (curShakeMag > MaxShakeMag)
                    curShakeMag = MaxShakeMag;

                Find.CameraDriver.JumpTo(Find.CameraDriver.MapPosition.ToVector3Shifted() + ShakeOffset);
            }
        }

        // called each tick after all other "updates", like tickers, drawing, debug info and so on
        public override void MapComponentUpdate()
        {
            if (curShakeMag > 0)
            {
                curShakeMag -= ShakeDecayRate * Time.deltaTime;

                if (curShakeMag < 0f)
                    curShakeMag = 0f;
            }
        }
    }
}
