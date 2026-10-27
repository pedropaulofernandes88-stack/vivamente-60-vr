# Registro de validação

## Ambiente de criação — 27/10/2026

- Arquivos e lógica revisados estaticamente.
- .NET runtime presente; SDK ausente. Testes console preparados, mas execução indisponível neste ambiente.
- Unity não localizado no caminho padrão. Importação, resolução de pacotes, compilação C#, aparência e APK ainda não validados.
- Nenhum teste em Meta Quest ou simulador executado.
- GitHub CLI informou autenticação inválida. Envio remoto pendente.

## Critérios antes de declarar protótipo VR funcional

- Importar sem erros no Unity de referência e registrar packages-lock.json e arquivos .meta gerados.
- Executar os testes do núcleo com SDK .NET.
- Criar a cena pelo menu e concluir uma rodada com mouse.
- Habilitar OpenXR e executar Project Validation sem problemas impeditivos.
- Gerar e instalar APK; confirmar tracking, escala e orientação com a pessoa sentada.
- Selecionar cada alvo com controles esquerdo/direito, sem duplicação por gatilho mantido.
- Pausar durante apresentação e resposta; garantir que progresso não avance; retomar e concluir.
- Encerrar durante apresentação; reiniciar sem animações ou contadores da rodada anterior.
- Confirmar números e instruções legíveis, distinguindo flores sem depender somente da cor.
- Verificar altura, alcance, fadiga, desconforto e facilidade de interromper com usuários; registrar resultados antes de expandir.

Até concluir estes itens, a entrega é uma base de implementação, não um aplicativo VR validado.
