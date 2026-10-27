using VivaMente.Core;

var tests = new (string Name, Action Body)[]
{
    ("Inicia mostrando a sequência", StartsInShowingState),
    ("Não aceita entrada antes da apresentação", RejectsInputBeforeShowingEnds),
    ("Erros não avançam a sequência", WrongSelectionDoesNotAdvance),
    ("Pausa bloqueia entrada", PauseBlocksInput),
    ("Conclui após o último acerto", CompletesAfterFinalSelection),
    ("Copia a sequência defensivamente", DefensivelyCopiesSequence),
    ("Reset restaura o estado inicial", ResetRestoresIdleState),
    ("Sequência vazia conclui ao encerrar apresentação", EmptySequenceCompletes)
};

var failures = new List<string>();
foreach (var test in tests)
{
    try
    {
        test.Body();
        Console.WriteLine($"PASS: {test.Name}");
    }
    catch (Exception exception)
    {
        failures.Add($"FAIL: {test.Name} — {exception.Message}");
    }
}

foreach (var failure in failures)
{
    Console.Error.WriteLine(failure);
}

return failures.Count == 0 ? 0 : 1;

static MemorySession Ready(int[] sequence)
{
    var session = new MemorySession();
    session.StartRound(sequence);
    session.FinishShowing();
    return session;
}

static void StartsInShowingState()
{
    var session = new MemorySession();
    session.StartRound(new[] { 1, 2 });
    Assert(session.State == MemorySessionState.Showing, "Estado deve ser Showing.");
    Assert(session.CurrentIndex == 0 && session.Mistakes == 0, "Contadores devem iniciar em zero.");
}

static void RejectsInputBeforeShowingEnds()
{
    var session = new MemorySession();
    session.StartRound(new[] { 4 });
    Assert(!session.Select(4), "Não deve aceitar entrada durante Showing.");
    Assert(session.CurrentIndex == 0, "Índice não deve avançar.");
}

static void WrongSelectionDoesNotAdvance()
{
    var session = Ready(new[] { 4, 2 });
    Assert(!session.Select(7), "Seleção diferente deve falhar.");
    Assert(session.CurrentIndex == 0, "Erro não pode avançar a sequência.");
    Assert(session.Mistakes == 1, "Erro deve ser contabilizado.");
}

static void PauseBlocksInput()
{
    var session = Ready(new[] { 4 });
    session.SetPaused(true);
    Assert(!session.Select(4), "Pausa deve bloquear seleções.");
    Assert(session.CurrentIndex == 0, "Índice não deve avançar em pausa.");
    session.SetPaused(false);
    Assert(session.Select(4), "Seleção deve voltar após retomar.");
}

static void CompletesAfterFinalSelection()
{
    var session = Ready(new[] { 4, 2 });
    Assert(session.Select(4), "Primeira seleção correta deve passar.");
    Assert(session.State == MemorySessionState.AwaitingInput, "Ainda deve esperar entrada.");
    Assert(session.Select(2), "Última seleção correta deve passar.");
    Assert(session.State == MemorySessionState.Completed, "Rodada deve ser concluída.");
    Assert(!session.Select(2), "Não deve aceitar seleção após conclusão.");
}

static void DefensivelyCopiesSequence()
{
    var sequence = new[] { 4, 2 };
    var session = new MemorySession();
    session.StartRound(sequence);
    sequence[0] = 9;
    session.FinishShowing();
    Assert(session.Select(4), "Alterar o array externo não pode mudar a rodada.");
}

static void ResetRestoresIdleState()
{
    var session = Ready(new[] { 4 });
    session.SetPaused(true);
    session.Reset();
    Assert(session.State == MemorySessionState.Idle, "Reset deve retornar a Idle.");
    Assert(session.CurrentIndex == 0 && session.Mistakes == 0 && !session.IsPaused, "Reset deve limpar os dados da rodada.");
}

static void EmptySequenceCompletes()
{
    var session = new MemorySession();
    session.StartRound(Array.Empty<int>());
    session.FinishShowing();
    Assert(session.State == MemorySessionState.Completed, "Sequência vazia deve concluir sem entrada.");
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
