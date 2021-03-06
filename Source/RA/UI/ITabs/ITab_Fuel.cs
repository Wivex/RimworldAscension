﻿using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using static RA.CommonTextures;

namespace RA
{
    public class ITab_Fuel : ITab
    {
        public CompFueled burner;

        public ITab_Fuel()
        {
            labelKey = "Fuel";
        }

        public override bool IsVisible => true;

        protected override void FillTab()
        {
            burner = SelThing.TryGetComp<CompFueled>();

            const float MarginSize = 5f;
            const float TextHeight = 25f;

            // height is the total count of used text field heights and margins
            size = new Vector2(432f, TextHeight * 6 + MarginSize * 3);

            // make smaller rect with margin size borders
            var innerRect = new Rect(0f, 0f, size.x, size.y).ContractedBy(MarginSize);
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.BeginGroup(innerRect);
            {
                // allowed fuel types filter button
                var filterRect = new Rect(0f, 0f, innerRect.width / 2, TextHeight);
                {
                    if (Widgets.ButtonText(filterRect, "Fuel filter"))
                    {
                        Find.WindowStack.Add(new Window_ThingFilter(burner, 105f));
                    }
                }

                // fuel container info
                var fuelRect = new Rect(0f, filterRect.height + MarginSize, filterRect.width, innerRect.height - (filterRect.height + MarginSize));
                // fuel container background
                Widgets.DrawMenuSection(fuelRect);
                // contract actual size after drawing BG
                fuelRect = fuelRect.ContractedBy(MarginSize);
                // if fuel tank not empty
                if (burner.fuelContainer.Count > 0)
                {
                    if (Widgets.ButtonInvisible(fuelRect))
                    {
                        var options = new List<FloatMenuOption>
                        {
                            new FloatMenuOption("Fuel Info",
                                () => { Find.WindowStack.Add(new Dialog_InfoCard(burner.fuelContainer[0])); }),
                            new FloatMenuOption("Fuel Drop".Translate(),
                                () => { burner.fuelContainer.TryDropAll(burner.parent.Position, ThingPlaceMode.Near); })
                        };

                        Find.WindowStack.Add(new FloatMenu(options, string.Empty));
                    }

                    GUI.BeginGroup(fuelRect);
                    {
                        var fuel = burner.fuelContainer[0];

                        // current fuel type icon
                        var fuelIconRect = new Rect(0f, 0f, TextHeight * 2, TextHeight * 2);
                        Widgets.DrawTextureFitted(fuelIconRect, IconBGTex, 1);
                        Widgets.ThingIcon(fuelIconRect.ContractedBy(2f), fuel);

                        // burning fillable bar
                        var burningLabelRect = new Rect(fuelIconRect.width + MarginSize, 0f, fuelRect.width - (fuelIconRect.width + MarginSize), fuelIconRect.height / 2);
                        Widgets.Label(burningLabelRect, "Burning progress:");
                        var burningBarRect = new Rect(burningLabelRect.x, burningLabelRect.height, burningLabelRect.width, burningLabelRect.height);
                        
                        var percentBurnDuration = burner.currentFuelBurnDuration / (fuel.GetStatValue(StatDef.Named("BurnDurationHours")) * GenDate.TicksPerHour);
                        Widgets.FillableBar(burningBarRect, percentBurnDuration, FullTexFuel, EmptyTex, false);

                        // fuel count fillable bar
                        var fuelCountBarRect = new Rect(0f, burningBarRect.yMax + MarginSize, fuelRect.width, 20f);
                        var fillPercentFuelCount = fuel.stackCount / (float)fuel.def.stackLimit;
                        Widgets.FillableBar(fuelCountBarRect, fillPercentFuelCount, FullTexFuelCount, EmptyTex, false);
                        Widgets.Label(fuelCountBarRect, string.Format("Fuel amount: {0}/{1}", fuel.stackCount, fuel.def.stackLimit));

                        Text.Anchor = TextAnchor.MiddleLeft;
                        // current fuel type info
                        var fuelEstimatedTimeRect = new Rect(0f, fuelCountBarRect.yMax + MarginSize / 2, fuelRect.width, 20f);
                        Widgets.Label(fuelEstimatedTimeRect, string.Format("Depletes after:\t{0}", TimeInfo(fuel.stackCount * (int)fuel.GetStatValue(StatDef.Named("BurnDurationHours")) * GenDate.TicksPerHour)));
                        // current fuel type info
                        var fuelMaxTempRect = new Rect(0f, fuelEstimatedTimeRect.yMax, fuelRect.width, fuelEstimatedTimeRect.height);
                        Widgets.Label(fuelMaxTempRect, string.Format("Max tempertarure:\t{0} °C", fuel.GetStatValue(StatDef.Named("MaxBurningTempCelsius"))));
                    }
                    GUI.EndGroup();
                }
                // no fuel
                else
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Text.Font = GameFont.Medium;
                    Widgets.Label(fuelRect, "No fuel");
                    Text.Font = GameFont.Small;
                }

                // burner info
                var burnerRect = new Rect(filterRect.width + MarginSize, 0f, innerRect.width - (fuelRect.width + MarginSize * 3), innerRect.height);
                GUI.BeginGroup(burnerRect);
                {
                    Text.Anchor = TextAnchor.MiddleCenter;

                    // refuel percent slider
                    var sliderLabelRect = new Rect(0f, 0f, burnerRect.width, TextHeight);
                    Widgets.Label(sliderLabelRect, "Refuel at " + burner.fuelStackRefuelPercent.ToStringPercent());
                    var sliderRect = new Rect(0f, sliderLabelRect.height - 2.5f, burnerRect.width, TextHeight / 2);
                    burner.fuelStackRefuelPercent = GUI.HorizontalSlider(sliderRect, burner.fuelStackRefuelPercent, 0f, 1f);

                    // burner fillable bar
                    var burnerLabelRect = new Rect(0f, sliderRect.yMax, burnerRect.width, TextHeight);
                    Widgets.Label(burnerLabelRect, "Internal temperature:");
                    var burnerBarRect = new Rect(0f, burnerLabelRect.yMax, burnerRect.width, TextHeight);
                    var percentRequiredHeat = Mathf.Min(burner.internalTemp / burner.compFueled.Properties.operatingTemp, 1f);
                    Widgets.FillableBar(burnerBarRect, percentRequiredHeat, percentRequiredHeat == 1 ? FullTexBurnerHight : FullTexBurnerLow, EmptyTex, false);
                    // line, indiciting operating temp, when internal is above that
                    if (percentRequiredHeat == 1)
                        Widgets.DrawLineVertical(burnerBarRect.x + burnerBarRect.width * (1 -burner.compFueled.Properties.operatingTemp / burner.internalTemp), burnerBarRect.y, burnerBarRect.height);
                    Widgets.Label(burnerBarRect, burner.internalTemp.ToString("F1") + " °C");

                    // burner operating temp label
                    var burnerOpLabelRect = new Rect(0f, burnerBarRect.yMax, burnerRect.width, TextHeight);
                    Widgets.Label(burnerOpLabelRect, "Operating temperature: " + burner.compFueled.Properties.operatingTemp +" °C");

                    // burner current condition label
                    var burnerConditionLabelRect = new Rect(0f, burnerOpLabelRect.yMax, burnerRect.width * 0.55f, burnerRect.height - burnerOpLabelRect.yMax);
                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(burnerConditionLabelRect, "Current status:");
                    // burner current condition status
                    var burnerConditionStatusRect = new Rect(burnerConditionLabelRect.xMax, burnerConditionLabelRect.y, burnerRect.width - burnerConditionLabelRect.width, burnerConditionLabelRect.height);
                    string status;
                    if (burner.internalTemp >= burner.compFueled.Properties.operatingTemp)
                    {
                        status = " working";
                        GUI.color = Color.green;
                    }
                    else
                    {
                        status = " low temp";
                        GUI.color = Color.red;
                    }
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(burnerConditionStatusRect, status);
                    GUI.color = Color.white;
                }
                GUI.EndGroup();
            }
            GUI.EndGroup();
            // resets text anchor to upper left (game default)
            GenUI.ResetLabelAlign();
        }

        public string TimeInfo(int ticks)
        {
            int years, months, days, hours;
            var timeInfo = new StringBuilder();
            if ((years = ticks / GenDate.TicksPerYear) > 0)
            {
                timeInfo.Append(years + "y ");
                ticks %= years;
            }
            if ((months = ticks / GenDate.TicksPerMonth) > 0)
            {
                timeInfo.Append(months + "m ");
                ticks %= months;
            }
            if ((days = ticks / GenDate.TicksPerDay) > 0)
            {
                timeInfo.Append(days + "d ");
                ticks %= days;
            }
            if ((hours = ticks / GenDate.TicksPerHour) > 0)
            {
                timeInfo.Append(hours + "h ");
            }
            if (years == 0 && months == 0 && days == 0 && hours == 0)
            {
                timeInfo.Append("<1h ");
            }
            return timeInfo.ToString();
        }
    }
}