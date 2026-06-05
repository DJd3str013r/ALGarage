# ADR-0011 — LGPD como fundação

- **Status:** Aceito
- **Data:** 2026-06-05
- **Decisores:** CTO, Tech Lead, PO

## Contexto
VIN identifica um veículo; ligado a uma conta (e potencialmente a placa/histórico) é **dado pessoal**
sob a LGPD. Tratar isso como _backlog_ é risco jurídico e de reputação.

## Decisão
LGPD é **requisito de fundação**, não feature futura. O esqueleto já prevê:
- **Residência de dados local** (servidor da equipe no Brasil — [ADR-0012](0012-hosting-deployment.md));
  se migrar para nuvem, escolher **região no Brasil**.
- **Base legal + consentimento** registrados (`ConsentLog`), com versão do termo.
- **Minimização**: só coletamos o necessário para a estimativa/serviço.
- **Direitos do titular**: exportação (`DataExportRequest`) e exclusão (`DeletionRequest`) — soft
  delete + rotina de exclusão definitiva.
- **Criptografia em repouso** no banco gerenciado; segredos fora do código.
- **Trilha de auditoria** mínima de acessos a dados sensíveis.

## Consequências
- ✅ Conformidade desde o início; menos retrabalho e risco.
- ⚠️ Alguma sobrecarga de fundação antes de features "visíveis" — justificada pelo risco.
- ➡️ Revisão jurídica recomendada antes do lançamento e antes de qualquer uso da marca "Volvo"
  (ver R8).
