﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RA
{
    [StaticConstructorOnStartup]
    public class RA_MainTabWindow_Research : MainTabWindow_Research
    {
        public Texture2D texSortByCost = ContentFinder<Texture2D>.Get("UI/Icons/SortByCost");
        public Texture2D texSortByName = ContentFinder<Texture2D>.Get("UI/Icons/SortByName");

        public static Texture2D BarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));
        public static Texture2D BarBgTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));

        public const float LeftAreaWidth = 330f;
        public const int ModeSelectButHeight = 40;
        public const float ProjectTitleHeight = 50f;
        public const float ProjectTitleLeftMargin = 20f;
        public const int ProjectIntervalY = 25;

        public enum ShowResearch
        {
            Available,
            Completed,
            All
        }
        public enum SortOptions
        {
            Name,
            Cost
        }

        public ShowResearch showResearchedProjects = ShowResearch.Available;
        public SortOptions currentSortOption = SortOptions.Cost;

        public Vector2 projectListScrollPosition = default(Vector2);
        public IEnumerable<ResearchProjectDef> researchProjectsList;
        
        public bool noBenchWarned;
        public bool sortAscending;
        public string currentFilter = "";
        public string previousFilter = "";

        public override void PreOpen()
        {
            base.PreOpen();

            RefreshList();
        }

        public override void DoWindowContents(Rect tabRect)
        {
            // MainTabWindow.DoWindowContents
            if (Anchor == MainTabWindowAnchor.Left)
            {
                windowRect.x = 0f;
            }
            else
            {
                windowRect.x = Screen.width - windowRect.width;
            }
            windowRect.y = Screen.height - 35 - windowRect.height;

            // throws message if no research bench is build yet
            if (!noBenchWarned)
            {
                if (!Find.ListerBuildings.ColonistsHaveResearchBench())
                {
                    Find.WindowStack.Add(new Dialog_Message("ResearchMenuWithoutBench".Translate()));
                }
                noBenchWarned = true;
            }

            // header
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(new Rect(0f, 0f, tabRect.width, 300f), "Research".Translate());
            UIUtil.ResetText();

            var rectSidePanel = new Rect(0f, 75f, 330f, tabRect.height - 75f);
            DrawResearchesListPanel(rectSidePanel);

            var rectDetailsPanel = new Rect(rectSidePanel.xMax + 10f, 45f, tabRect.width - rectSidePanel.width - 10f, tabRect.height - 45f);
            DrawDetailsPanel(rectDetailsPanel);

        }

        public void DrawResearchesListPanel(Rect rectSidePanel)
        {
            Widgets.DrawMenuSection(rectSidePanel, false);

            // controls row that adds search textfield and sorter
            var rectControlsRow = rectSidePanel.ContractedBy(10f);
            rectControlsRow.height = 30f;

            var rectTextFilter = new Rect(rectControlsRow) {width = rectControlsRow.width - 110f};
            var rectClearFilterButton = new Rect(rectTextFilter.xMax + 6f, rectTextFilter.yMin + 3f, 24f, 24f);
            var rectSortByNameButton = new Rect(rectClearFilterButton.xMax + 6f, rectTextFilter.yMin + 3f, 24f, 24f);
            var rectSortByCostButton = new Rect(rectSortByNameButton.xMax + 6f, rectTextFilter.yMin + 3f, 24f, 24f);

            // draw filter text field, return input
            currentFilter = Widgets.TextField(rectTextFilter, currentFilter);
            if (previousFilter != currentFilter)
            {
                previousFilter = currentFilter;
                RefreshList();
            }
            // if any text in the field
            if (currentFilter != "")
            {
                // draw clear filter text field button
                if (Widgets.ButtonImage(rectClearFilterButton, Widgets.CheckboxOffTex))
                {
                    currentFilter = "";
                    RefreshList();
                }
            }

            // draw name sorter button
            if (Widgets.ButtonImage(rectSortByNameButton, texSortByName))
            {
                // if other sort option was selected before
                if (currentSortOption != SortOptions.Name)
                {
                    // do the ascending sort by cost
                    currentSortOption = SortOptions.Name;
                    sortAscending = true;
                    RefreshList();
                }
                // else inverse the sorting method for the current sort option
                else
                {
                    sortAscending = !sortAscending;
                    RefreshList();
                }
            }
            // draw cost sorter button
            if (Widgets.ButtonImage(rectSortByCostButton, texSortByCost))
            {
                if (currentSortOption != SortOptions.Cost)
                {
                    currentSortOption = SortOptions.Cost;
                    sortAscending = true;
                    RefreshList();
                }
                else
                {
                    sortAscending = !sortAscending;
                    RefreshList();
                }
            }

            // research projects list
            var rectResearchesList = rectSidePanel.ContractedBy(10f);
            rectResearchesList.yMin += 30f;
            rectResearchesList.height -= 30f;
            float height = 25 * researchProjectsList.Count() + 100;
            var sidebarContent = new Rect(0f, 0f, rectResearchesList.width - 16f, height);
            Widgets.BeginScrollView(rectResearchesList, ref projectListScrollPosition, sidebarContent);
            var position = sidebarContent.ContractedBy(10f);
            GUI.BeginGroup(position);
            var num = 0;

            foreach (var current in researchProjectsList)
            {
                var rectCurrentProject = new Rect(0f, num, position.width, 25f);
                if (selectedProject == current)
                {
                    GUI.DrawTexture(rectCurrentProject, TexUI.HighlightTex);
                }

                var text = current.LabelCap + " (" + current.CostApparent.ToString("F0") + ")";
                var sidebarRowInner = new Rect(rectCurrentProject);
                sidebarRowInner.x += 6f;
                sidebarRowInner.width -= 6f;
                var num2 = Text.CalcHeight(text, sidebarRowInner.width);
                if (sidebarRowInner.height < num2)
                {
                    sidebarRowInner.height = num2 + 3f;
                }

                // give the label a colour if we're in the all categories.
                Color textColor;
                if (showResearchedProjects == ShowResearch.All)
                {
                    if (current.IsFinished)
                    {
                        textColor = new Color(1f, 1f, 1f);
                    }
                    else if (!current.PrerequisitesCompleted)
                    {
                        textColor = new Color(.6f, .6f, .6f);
                    }
                    else
                    {
                        textColor = new Color(.8f, .85f, 1f);
                    }
                }
                else
                {
                    textColor = new Color(.8f, .85f, 1f);
                }
                if (Widgets.ButtonText(sidebarRowInner, text, false, true, textColor))
                {
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    selectedProject = current;
                }
                num += 25;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();

            // draw research tabs selection
            var list = new List<TabRecord>
            {
                new TabRecord("Available", () =>
                {
                    showResearchedProjects = ShowResearch.Available;
                    RefreshList();
                }, showResearchedProjects == ShowResearch.Available),
                new TabRecord("Researched", () =>
                {
                    showResearchedProjects = ShowResearch.Completed;
                    RefreshList();
                }, showResearchedProjects == ShowResearch.Completed)
            };
            if (Prefs.DevMode)
            {
                list.Add(new TabRecord("All", () =>
                {
                    showResearchedProjects = ShowResearch.All;
                    RefreshList();
                }, showResearchedProjects == ShowResearch.All));
                TabDrawer.DrawTabs(rectSidePanel, list);
            }
        }

        public void DrawDetailsPanel(Rect rectDetailsPanel)
        {
            Widgets.DrawMenuSection(rectDetailsPanel);

            var rectInnerDetailsPanel = rectDetailsPanel.ContractedBy(20f);
            GUI.BeginGroup(rectInnerDetailsPanel);
            if (selectedProject != null)
            {
                // selected project label
                Text.Font = GameFont.Medium;
                GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
                var rectResearchLabel = new Rect(20f, 0f, rectInnerDetailsPanel.width - 20f, 50f);
                Widgets.Label(rectResearchLabel, selectedProject.LabelCap);
                GenUI.ResetLabelAlign();
                Text.Font = GameFont.Small;

                // draw project description
                var rectDescription = new Rect(0f, 50f, rectInnerDetailsPanel.width, rectInnerDetailsPanel.height - 50f);
                var descriptionString = new StringBuilder();
                descriptionString.Append(selectedProject.description + "\n\n");
                var researchRecipe = DefDatabase<RecipeDef>.GetNamedSilentFail(selectedProject.defName);
                // if there are required ingridients
                if (researchRecipe != null && !researchRecipe.ingredients.NullOrEmpty())
                {
                    // draw research prerequisites
                    descriptionString.Append("Required research materials:\n");
                    foreach (var ingridient in researchRecipe.ingredients)
                    {
                        descriptionString.AppendLine("\t" + ingridient);

                        //descriptionString.AppendFormat("\t{0} ", ingridient.GetBaseCount());

                        //descriptionString.Append(ingridient.GetBaseCount() == 1 ? "sample of " : "samples of ");

                        //if (!ingridient.filter.)
                        //{
                        //    descriptionString.AppendFormat("of {0} category\n", ThingCategoryDef.Named(ingridient.filter.categories[0]).LabelCap);
                        //}
                        //else
                        //    descriptionString.AppendFormat("{0}\n", ingridient.filter.thingDefs[0].LabelCap);
                    }
                }
                Widgets.Label(rectDescription, descriptionString.ToString());

                var rectResearchButton = new Rect(rectInnerDetailsPanel.width / 2f - 50f, 300f, 100f, 50f);
                if (selectedProject.IsFinished)
                {
                    Widgets.DrawMenuSection(rectResearchButton);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(rectResearchButton, "Finished".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else if (selectedProject == Find.ResearchManager.currentProj)
                {
                    Widgets.DrawMenuSection(rectResearchButton);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(rectResearchButton, "InProgress".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else if (!selectedProject.PrerequisitesCompleted)
                {
                    Widgets.DrawMenuSection(rectResearchButton);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(rectResearchButton, "Locked");
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else
                {
                    if (Widgets.ButtonText(rectResearchButton, "Research".Translate()))
                    {
                        SoundDef.Named("ResearchStart").PlayOneShotOnCamera();
                        Find.ResearchManager.currentProj = selectedProject;
                    }
                    if (Prefs.DevMode)
                    {
                        var rectInstaResearchButton = rectResearchButton;
                        rectInstaResearchButton.x += rectInstaResearchButton.width + 4f;
                        if (Widgets.ButtonText(rectInstaResearchButton, "Debug Insta-finish"))
                        {
                            Find.ResearchManager.currentProj = selectedProject;
                            Find.ResearchManager.InstantFinish(selectedProject);
                        }
                    }
                }

                var rectProgressBar = new Rect(15f, 450f, rectInnerDetailsPanel.width - 30f, 35f);
                Widgets.FillableBar(rectProgressBar, selectedProject.ProgressPercent, BarFillTex, BarBgTex, true);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rectProgressBar, selectedProject.ProgressApparent.ToString("F0") + " / " + selectedProject.CostApparent.ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }
            GUI.EndGroup();
        }

        // define what research defs to draw
        public void RefreshList()
        {
            switch (showResearchedProjects)
            {
                case ShowResearch.All:
                    researchProjectsList = DefDatabase<ResearchProjectDef>.AllDefs.Where(projectDef => !projectDef.IsFinished && projectDef.prerequisites.Contains(ResearchProjectDef.Named("Blocked")));
                    break;
                case ShowResearch.Completed:
                    researchProjectsList = DefDatabase<ResearchProjectDef>.AllDefs.Where(projectDef => projectDef.IsFinished);
                    break;
                default:
                    researchProjectsList = DefDatabase<ResearchProjectDef>.AllDefs.Where(projectDef => !projectDef.IsFinished && (projectDef.PrerequisitesCompleted || projectDef == ResearchProjectDef.Named("Survival") && Prefs.DevMode));
                    break;
            }

            if (currentFilter != "")
            {
                researchProjectsList = researchProjectsList.Where(projectDef => projectDef.label.ToUpper().Contains(currentFilter.ToUpper()));
            }

            switch (currentSortOption)
            {
                case SortOptions.Cost:
                    researchProjectsList = researchProjectsList.OrderBy(projectDef => projectDef.CostApparent);
                    break;
                case SortOptions.Name:
                    researchProjectsList = researchProjectsList.OrderBy(projectDef => projectDef.LabelCap);
                    break;
            }

            if (sortAscending) researchProjectsList = researchProjectsList.Reverse();
        }
    }
}