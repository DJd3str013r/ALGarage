namespace ALGarage.Web;

/// <summary>
/// Classe-marcadora para localização compartilhada. Injete <c>IStringLocalizer&lt;SharedResource&gt;</c>
/// nos componentes e use as chaves dos arquivos em <c>/Resources</c>:
/// <c>SharedResource.resx</c> (en, neutro) e <c>SharedResource.pt-BR.resx</c>.
/// Fica na raiz do projeto de propósito: com <c>ResourcesPath = "Resources"</c>, o sistema procura
/// os recursos em <c>Resources/SharedResource.*.resx</c>. Ver ADR-0013.
/// </summary>
public sealed class SharedResource;
