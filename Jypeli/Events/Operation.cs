using System;

namespace Jypeli
{
    /// <summary>
    /// Käynnissä oleva tehtävä
    /// </summary>
    public interface Operation
    {
        /// <summary>
        /// Onko tehtävä käynnissä.
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// Pysäyttää tehtävän.
        /// </summary>
        void Stop();

        /// <summary>
        /// Tapahtuu kun tehtävä valmistuu.
        /// Ei tapahdu, jos tehtävä keskeytetään Stop-aliohjelmalla.
        /// </summary>
        event Action Finished;

        /// <summary>
        /// Tapahtuu kun tehtävä pysäytetään Stop-metodilla.
        /// </summary>
        event Action Stopped;
    }
}
