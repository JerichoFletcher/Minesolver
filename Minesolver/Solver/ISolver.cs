namespace Minesolver.Solver {
    internal interface ISolver {
        void Solve();
        void SolveImmediate(int attemptCount);
    }
}
