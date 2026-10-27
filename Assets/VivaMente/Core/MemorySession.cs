using System;

namespace VivaMente.Core
{
    public enum MemorySessionState
    {
        Idle,
        Showing,
        AwaitingInput,
        Completed
    }

    /// <summary>
    /// Regras puras do exercício de sequência. A camada Unity controla apenas a
    /// apresentação visual, som e os controles de interação.
    /// </summary>
    public sealed class MemorySession
    {
        private int[] sequence = Array.Empty<int>();

        public MemorySessionState State { get; private set; } = MemorySessionState.Idle;
        public int CurrentIndex { get; private set; }
        public int Mistakes { get; private set; }
        public bool IsPaused { get; private set; }

        public void StartRound(int[] newSequence)
        {
            if (newSequence == null)
            {
                throw new ArgumentNullException(nameof(newSequence));
            }

            sequence = (int[])newSequence.Clone();
            CurrentIndex = 0;
            Mistakes = 0;
            IsPaused = false;
            State = MemorySessionState.Showing;
        }

        public void FinishShowing()
        {
            if (State == MemorySessionState.Showing && !IsPaused)
            {
                State = sequence.Length == 0
                    ? MemorySessionState.Completed
                    : MemorySessionState.AwaitingInput;
            }
        }

        /// <summary>
        /// Registra uma seleção. Retorna true somente quando a seleção está
        /// disponível e corresponde ao próximo item da sequência.
        /// </summary>
        public bool Select(int selection)
        {
            if (State != MemorySessionState.AwaitingInput || IsPaused)
            {
                return false;
            }

            if (selection != sequence[CurrentIndex])
            {
                Mistakes++;
                return false;
            }

            CurrentIndex++;
            if (CurrentIndex == sequence.Length)
            {
                State = MemorySessionState.Completed;
            }

            return true;
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused && (State == MemorySessionState.Showing || State == MemorySessionState.AwaitingInput);
        }

        public void Reset()
        {
            sequence = Array.Empty<int>();
            CurrentIndex = 0;
            Mistakes = 0;
            IsPaused = false;
            State = MemorySessionState.Idle;
        }
    }
}
