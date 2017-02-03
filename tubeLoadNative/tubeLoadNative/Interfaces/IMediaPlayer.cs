using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tubeLoadNative.Interfaces
{
    public interface IMediaPlayer
    {
        #region Events

        event EventHandler OnComplete;

        #endregion

        #region Props

        int Duration { get; }

        int CurrentPosition { get; }

        bool IsPlaying { get; }

        bool HasDataSource { get; set; }

        #endregion

        #region Methods

        bool Start(string path);
        void Pause();
        void Continue();
        void Stop();
        void SeekTo(int position);

        #endregion
    }
}
