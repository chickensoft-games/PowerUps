namespace Chickensoft.PowerUps.Tests.Fixtures;

using System;

public class DisposableObject : IDisposable {
  public bool IsDisposed { get; private set; }

  public void Dispose() {
    IsDisposed = true;
    GC.SuppressFinalize(this);
  }
}
