# ADR-0004 — ASP.NET Core Identity (OIDC-ready)

- **Status:** Aceito
- **Data:** 2026-06-05
- **Decisores:** CTO, Tech Lead

## Contexto
Precisamos de contas de usuário com VIN obrigatório no cadastro. LGPD nos pressiona a controlar o
dado do usuário. Custo importa no MVP.

## Opções consideradas
1. **ASP.NET Core Identity** — embutido, sem custo por usuário, donos do dado. Custo: implementar
   reset de senha, e-mail, MFA nós mesmos.
2. **IdP externo (Auth0/Entra ID/Clerk)** — menos código, MFA/social prontos. Custo: preço por MAU,
   dado do usuário fora, dependência externa.
3. **Keycloak self-hosted** — OIDC completo e livre, mas mais um serviço para operar no MVP.

## Decisão
**ASP.NET Core Identity** para o MVP, **atrás de uma abstração OIDC-ready**, permitindo plugar IdP
externo ou login social depois sem reescrever o domínio.

## Consequências
- ✅ Custo zero por usuário; controle total do dado (bom p/ LGPD).
- ✅ Caminho de migração para OIDC preservado.
- ⚠️ Responsabilidade de segurança (hashing está pronto, mas e-mail/MFA/reset são nossos).
- ➡️ Reavaliar IdP externo quando a base crescer ou exigências de MFA/social aumentarem.
