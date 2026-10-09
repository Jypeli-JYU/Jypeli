using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Jypeli.Profiling;
#if PROFILE
using Silk.NET.OpenGL.Extensions.ImGui;
#endif
namespace Jypeli
{
    public partial class Game
    {
        // Real time passed, including paused time
        private static Time currentRealTime = new Time();

        // Game time passed
        private static Time currentTime = new Time();

        /// <summary>
        /// Ajetaanko peliä kiinteällä aika-askeleella.
        /// Eli pelin aika "hidastuu" jos tietokoneen tehot eivät riitä reaaliaikaiseen päivitykseen.
        /// Tämä pakottaa pelin logiikan toimimaan sillä nopeudella kuin <see cref="UpdatesPerSecod"/> on asetettu.
        /// </summary>
        public static bool FixedTimeStep { get; set; }

        /// <summary>
        /// Kuinka monta pelipäivitystä ajetaan sekunnissa.
        /// Vakiona 60.
        /// Tällä hetkellä myös pelin FPS-tavoite asettuu samaan arvoon.
        /// Tämän muuttaminen "vääräksi" arvoksi saattaa aiheuttaa erikoisia seurauksia jos <see cref="FixedTimeStep"/> on päällä.
        /// </summary>
        [Obsolete("-> UpdatesPerSecond")]
        public static double UpdatesPerSecod
        {
            get => Instance.Window.UpdatesPerSecond;
            set
            {
                Instance.Window.UpdatesPerSecond = value;
                Instance.Window.FramesPerSecond = value;
            }
        }

        /// <summary>
        /// Kuinka monta pelipäivitystä ajetaan sekunnissa.
        /// Vakiona 60.
        /// Tällä hetkellä myös pelin FPS-tavoite asettuu samaan arvoon.
        /// Tämän muuttaminen "vääräksi" arvoksi saattaa aiheuttaa erikoisia seurauksia jos <see cref="FixedTimeStep"/> on päällä.
        /// </summary>
        public static double UpdatesPerSecond
        {
            get => UpdatesPerSecod;
            set
            {
                UpdatesPerSecod = value;
            }
        }

        /// <summary>
        /// Onko peli pysähdyksissä.
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        /// Peliaika. Sisältää tiedon siitä, kuinka kauan peliä on pelattu (Time.SinceStartOfGame)
        /// ja kuinka kauan on viimeisestä pelin päivityksestä (Time.SinceLastUpdate).
        /// Tätä päivitetään noin 30 kertaa sekunnissa kun peli ei ole pause-tilassa.
        /// </summary>
        public static Time Time
        {
            get { return currentTime; }
        }

        /// <summary>
        /// Todellinen peliaika. Sisältää tiedon siitä, kuinka kauan peliä on pelattu (Time.SinceStartOfGame)
        /// ja kuinka kauan on viimeisestä pelin päivityksestä (Time.SinceLastUpdate).
        /// Tätä päivitetään noin 30 kertaa sekunnissa, myös pause-tilassa.
        /// </summary>
        public static Time RealTime
        {
            get { return currentTime; }
        }

        /// <summary>
        /// Asettaa pelin pauselle, tai jatkaa peliä.
        /// Toimii samoin kuin IsPaused-ominaisuus
        /// </summary>
        public void Pause()
        {
            IsPaused = !IsPaused;
        }

        /// <summary>
        /// Poistaa kaikki ajastimet.
        /// </summary>
        public void ClearTimers()
        {
            Timer.ClearAll();
        }

        /// <summary>
        /// Ajetaan Updaten sijaan kun peli on pysähdyksissä.
        /// </summary>
        protected virtual void PausedUpdate(Time time)
        {
            foreach (var layer in Layers)
            {
                // Update the UI components only
                layer.Objects.Update(time, o => o is Widget);
            }

            Timer.UpdateAll(time, t => t.IgnorePause);
            UpdateControls(time);
        }

        /// <summary>
        /// Kun pelitilanne pitää päivittää.
        /// </summary>
        /// <param name="dt">Kulunut aika edellisestä päivityksestä</param>
        protected void OnUpdate(double dt)
        {
            Profiler.Start("Update");
            // Jos jostain syystä olisi tulossa hyvin iso dt, muutetaan se pieneksi.
            // Tämä voi tapahtua esim kun ikkunaa raahataan. Raahauksen aikana ei ajeta päivityksiä,
            // kun raahaus päättyy tulee päivitys hyvin suurella dt-arvolla, joka taas rikkoo fysiikoita.
            if (dt > 0.5)
                dt = 1 / 60.0;

            if (!IsPaused)
            {
                if (FixedTimeStep)
                    currentTime.Advance(1 / Window.UpdatesPerSecond);
                else
                    currentTime.Advance(dt);
                Update(currentTime);
            }
            else
            {
                PausedUpdate(currentRealTime);
            }

            Profiler.End();
            var lista = Profiler.Steps;
        }

        /// <summary>
        /// Kun pelitilanne pitää piirtää ruudulle
        /// </summary>
        /// <param name="dt">Kulunut aika edellisestä päivityksestä</param>
        protected void OnDraw(double dt)
        {
            Profiler.Start("Draw");
            if (!Closing) // Jos peli ollaan sulkemassa, ei yritetä piirtää.
                Draw(Time);
            Profiler.End();

            DrawProfiler(dt);
        }

        [Conditional("PROFILE")]
        private void DrawProfiler(double dt)
        {
#if PROFILE
            imguiController.Update((float)dt);

            ImGuiNET.ImGui.Begin("Profiler");

            var updates = Profiler.Steps["Update"].Timings.Select(s => (float)s.Duration).ToArray();
            var draws = Profiler.Steps["Draw"].Timings.Select(s => (float)s.Duration).ToArray();

            string MinMaxStr(CyclicArray<Timing> timings)
            {
                return $"{timings.Min(t => t.Duration):F2} / {timings.Average(t => t.Duration):F2} / {timings.Max(t => t.Duration):F2}";
            }

            if (updates.Length > 0)
            {
                ImGuiNET.ImGui.PlotLines("Update", ref updates[0], updates.Length, 0, null, 2, Math.Max(updates.Max(), 5), new System.Numerics.Vector2(500, 50));
                ImGuiNET.ImGui.SameLine();
                ImGuiNET.ImGui.Text($"{MinMaxStr(Profiler.Steps["Update"].Timings)} ms");

                ImGuiNET.ImGui.PlotLines("Draw", ref draws[0], draws.Length, 0, null, 2, Math.Max(updates.Max(), 5), new System.Numerics.Vector2(500, 50));
                ImGuiNET.ImGui.SameLine();
                ImGuiNET.ImGui.Text($"{MinMaxStr(Profiler.Steps["Draw"].Timings)}ms");
            }

            void PrintProfiler(Step node)
            {
                foreach (var item in node.SubSteps.Values)
                {
                    if (ImGuiNET.ImGui.TreeNode($"{item.Name} {MinMaxStr(item.Timings)}###{item.Name}"))
                    {
                        PrintProfiler(item);
                    }
                }
                ImGuiNET.ImGui.TreePop();
            }

            foreach (var item in Profiler.Steps.Values)
            {
                if (ImGuiNET.ImGui.TreeNode($"{item.Name} {MinMaxStr(item.Timings)}###{item.Name}"))
                {
                    PrintProfiler(item);
                }
            }

            ImGuiNET.ImGui.End();
            imguiController.Render();
#endif
        }

        /// <summary>
        /// Ajetaan kun pelin tilannetta päivitetään. Päivittämisen voi toteuttaa perityssä luokassa
        /// toteuttamalla tämän metodin. Perityn luokan metodissa tulee kutsua kantaluokan metodia.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void Update(Time time)
        {
            Profiler.BeginStep("Controls");
            UpdateControls(time);
            Profiler.EndStep();

            if (DataStorage.IsUpdated)
                DataStorage.Update(currentRealTime);

            Profiler.BeginStep("Camera");
            Camera.Update(time);
            Profiler.EndStep();

            Profiler.BeginStep("Layers");
            Layers.Update(time);
            Profiler.EndStep();

            Profiler.BeginStep("Timers");
            Timer.UpdateAll(time);
            Profiler.EndStep();

            Profiler.BeginStep("DebugScreen");
            UpdateDebugScreen(currentRealTime);
            Profiler.EndStep();

            Profiler.BeginStep("Handlers");
            UpdateHandlers(time);
            Profiler.EndStep();

            Profiler.BeginStep("PendingActions");
            ExecutePendingActions();
            Profiler.EndStep();
        }
    }
}
