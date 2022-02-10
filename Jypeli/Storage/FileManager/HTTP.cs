using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace Jypeli
{
    public partial class FileManager : Updatable
    {
        internal static HttpClient Client = new HttpClient();
        public class AsyncOperation
        {
            public Task<HttpResponseMessage> Request;
            public IAsyncResult Result;
            public TimeSpan Lifetime;
            public Action<StorageFile> Callback;
            public CancellationTokenSource CancellationToken;

            public bool IsCompleted
            {
                get { return Result == null ? true : Result.IsCompleted; }
            }

            public AsyncOperation(Task<HttpResponseMessage> req, Action<StorageFile> callback)
            {
                this.Request = req;
                this.Callback = callback;
                this.Lifetime = TimeSpan.FromSeconds(15);
            }

            public AsyncOperation(Task<HttpResponseMessage> req, Action<StorageFile> callback, TimeSpan timeout)
            {
                this.Request = req;
                this.Callback = callback;
                this.Lifetime = timeout;
            }

            public AsyncOperation(Task<HttpResponseMessage> req, CancellationTokenSource cancel, Action<StorageFile> callback)
            {
                this.Request = req;
                this.CancellationToken = cancel;
                this.Callback = callback;
                this.Lifetime = TimeSpan.FromSeconds( 15 );
            }

            public AsyncOperation(Task<HttpResponseMessage> req, CancellationTokenSource cancel, Action<StorageFile> callback, TimeSpan timeout)
            {
                this.Request = req;
                this.CancellationToken = cancel;
                this.Callback = callback;
                this.Lifetime = timeout;
            }
        }

        private class AsyncTrigger
        {
            public List<AsyncOperation> ops;
            public Action onCompleted;
            public int triggered = 0;
            public int failed = 0;

            public AsyncTrigger( List<AsyncOperation> operations, Action endAction )
            {
                this.ops = operations;
                this.onCompleted = endAction;
            }

            public AsyncTrigger( List<AsyncOperation> operations, TimeSpan timeout, Action endAction )
            {
                this.ops = operations;
                this.onCompleted = endAction;
                foreach ( var op in ops ) op.Lifetime = timeout;
            }
        }
        
        SynchronousList<AsyncTrigger> triggers = new SynchronousList<AsyncTrigger>();

        public bool IsUpdated
        {
            get { return triggers.Count > 0; }
        }

        public void Update( Time time )
        {
            triggers.Update( time );

            foreach ( var trig in triggers )
            {
                foreach ( var op in trig.ops.FindAll( o => !o.IsCompleted ) )
                {
                    op.Lifetime -= time.SinceLastUpdate;

                    if ( op.Lifetime.TotalSeconds <= 0 )
                    {
                        //Game.Instance.MessageDisplay.Add( "!!ABORT" );
                        op.CancellationToken.Cancel();
                        trig.failed++;
                    }
                }

                if ( trig.triggered + trig.failed >= trig.ops.Count )
                {
                    triggers.Remove( trig );
                    trig.onCompleted();
                }
            }
        }

        /// <summary>
        /// Avaa tiedoston (lukua varten) ja tekee sillä jotain.
        /// </summary>
        /// <param name="fileName">Tiedoston nimi</param>
        /// <param name="callback">Mitä tehdään (aliohjelman nimi)</param>
        /// <example>
        /// {
        ///    DoWith( "kuva.png", AsetaKuva );
        /// }
        /// 
        /// void AsetaKuva( StorageFile kuva )
        /// {
        ///    olio.Image = new Image( kuva );
        /// }
        /// </example>
        public AsyncOperation DoWith( string fileName, Action<StorageFile> callback )
        {
            using ( var f = Open( fileName, false ) )
            {
                callback( f );
            }

            return new AsyncOperation( null, callback );
        }

        /// <summary>
        /// Avaa tiedoston netistä (lukua varten) ja tekee sillä jotain.
        /// </summary>
        /// <param name="url">Nettiosoite</param>
        /// <param name="callback">Mitä tehdään (aliohjelman nimi)</param>
        /// <example>
        /// {
        ///    DoWith( "http://www.google.fi/images/srpr/logo3w.png", AsetaKuva );
        /// }
        /// 
        /// void AsetaKuva( StorageFile kuva )
        /// {
        ///    olio.Image = new Image( kuva );
        /// }
        /// </example>
        public AsyncOperation DoWithURL( string url, Action<StorageFile> callback )
        {
            var cancelToken = new CancellationTokenSource();
            var request = Client.GetAsync(url, cancelToken.Token);
            request.ContinueWith((t) =>
            {
                callback(new StorageFile(url, request.Result.Content.ReadAsStream()));
            });
            AsyncOperation op = new AsyncOperation(request, cancelToken, callback);
            return op;
        }

        /// <summary>
        /// Avaa tiedoston netistä (lukua varten) ja tekee sillä jotain.
        /// </summary>
        /// <param name="url">Nettiosoite</param>
        /// <param name="timeout">Paljonko aikaa tiedoston lataamiselle annetaan. Mikäli
        /// lataaminen ei onnistu annetussa ajassa, se keskeytetään.</param>
        /// <param name="callback">Mitä tehdään (aliohjelman nimi)</param>
        /// <example>
        /// {
        ///    DoWithURL( "http://www.google.fi/images/srpr/logo3w.png", AsetaKuva );
        /// }
        /// 
        /// void AsetaKuva( StorageFile kuva )
        /// {
        ///    olio.Image = new Image( kuva );
        /// }
        /// </example>
        public AsyncOperation DoWithURL( string url, TimeSpan timeout, Action<StorageFile> callback )
        {
            var cancelToken = new CancellationTokenSource();
            cancelToken.CancelAfter(timeout);
            var request = Client.GetAsync(url, cancelToken.Token);
            request.ContinueWith((t) =>
            {
                callback(new StorageFile(url, request.Result.Content.ReadAsStream()));
            });
            AsyncOperation op = new AsyncOperation(request, callback, timeout);
            return op;
        }

        /// <summary>
        /// Laukaisee aliohjelman kun annetut operaatiot on suoritettu.
        /// </summary>
        /// <param name="callback">Aliohjelma</param>
        /// <param name="actions">Operaatiot</param>
        public void TriggerOnComplete( Action callback, params AsyncOperation[] actions )
        {
            triggers.Add( new AsyncTrigger( actions.ToList().FindAll( o => !o.IsCompleted ), callback ) );
            triggers.Update( Game.Time );
        }

        /// <summary>
        /// Laukaisee aliohjelman kun annetut operaatiot on suoritettu.
        /// </summary>
        /// <param name="callback">Aliohjelma</param>
        /// <param name="timeout">Kuinka pitkään odotetaan yksittäistä operaatiota ennen kuin luovutetaan</param>
        /// <param name="actions">Operaatiot</param>
        public void TriggerOnComplete( Action callback, TimeSpan timeout, params AsyncOperation[] actions )
        {
            triggers.Add( new AsyncTrigger( actions.ToList().FindAll( o => !o.IsCompleted ), timeout, callback ) );
            triggers.Update( Game.Time );
        }
    }
}
