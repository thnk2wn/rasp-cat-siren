using System;
using System.Diagnostics;

namespace CatSiren
{
    public abstract class DisposableObject : IDisposable
    {
        ~DisposableObject()
        {
            if (!Disposed)
            {
                Debug.WriteLine(
                    $"WARNING: object {GetType().FullName} finalized without being disposed!");
            }

            Dispose(false);
        }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeManagedResources()
        {
        }

        protected virtual void DisposeUnmanagedResources()
        {
        }

        private void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                }

                DisposeUnmanagedResources();
                Disposed = true;
            }
        }
    }
}