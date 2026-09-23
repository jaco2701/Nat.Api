using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Static;
using Nat.API.Models.BR;
using Nat.API.Properties;
using Newtonsoft.Json;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
//using Microsoft.Playwright;
namespace Applet.Nat.Api.Br.Models
{
    public class Cuit
    {
        #region CONSTRUCT
        public Cuit() { }
        public Cuit(long vivlngCuit, NatContext vioContext, IConfiguration? vioConfiguration = null)
        {
            mioContext = vioContext;
            if (vioConfiguration != null)
                mioConfiguration = vioConfiguration;
            CuitModel lioCuitModel = mioContext.Cuits.Find(vivlngCuit);
            if (lioCuitModel == null)
                throw new Exception($"CUIT {vivlngCuit} {Resources.lioE_ObjectNoM}");
            ioDcModel = lioCuitModel;
        }
        public Cuit(CuitModel vioCuitModel, NatContext vioContext, IConfiguration vioConfiguration)
        {
            mioConfiguration = vioConfiguration;
            mioContext = vioContext;
            ioDcModel = vioCuitModel;
        }
        #endregion
        #region PUBLIC PROPS
        public CuitModel ioDcModel { get; set; }
        public CuitCnfg? ioCnfg
        {
            get
            {
                return JsonConvert.DeserializeObject<CuitCnfg>(Encoding.UTF8.GetString(Convert.FromBase64String(Format.UnCompress(ioDcModel.ivstrCnfg ?? string.Empty, Encoding.UTF8))));
            }
        }
        public eTask ieTask { get; set; }
        #endregion
        #region PRIVATE PROPS
        private IConfiguration mioConfiguration;
        private NatContext mioContext;
        #endregion
        #region PUBLICS METHODS
        public void SetDC(NatContext vioContext)
        {
            mioContext = vioContext;
        }

        public async Task Task()
        {
            switch (ieTask)
            {
                case eTask.Delete:
                    mioContext.Cuits.Remove(ioDcModel);
                    mioContext.SaveChanges();
                    break;
                case eTask.Save:
                    CuitModel lioDbCuitModel = mioContext.Cuits.Find(ioDcModel.ivlngCuit);
                    if (lioDbCuitModel != null)
                    {
                        lioDbCuitModel.ivstrCuitRS = ioDcModel.ivstrCuitRS;
                        lioDbCuitModel.ivstrCnfg = ioDcModel.ivstrCnfg;
                        mioContext.Cuits.Update(lioDbCuitModel);
                    }
                    else
                        mioContext.Cuits.Add(ioDcModel);
                    mioContext.SaveChanges();
                    break;
                case eTask.GetDocRec:
                    //string cuit = "20123456789";
                    //string claveFiscal = "TuClaveSegura123";
                    //string fechaDesde = "01/02/2026"; 
                    //string fechaHasta = "28/02/2026";

                    //using var playwright = await Playwright.CreateAsync();

                    //// Lanzamos el navegador (Headless = false para ver lo que hace el robot en desarrollo)
                    //await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    //{
                    //    Headless = false,
                    //    SlowMo = 500 // Retraso de 500ms entre acciones para simular comportamiento humano
                    //});

                    //var context = await browser.NewContextAsync();
                    //var page = await context.NewPageAsync();

                    //try
                    //{
                    //    Console.WriteLine("1. Iniciando sesión en ARCA...");
                    //    await page.GotoAsync("https://afip.gob.ar");

                    //    // Ingreso de CUIT
                    //    await page.FillAsync("#F1\\:username", cuit);
                    //    await page.ClickAsync("#F1\\:btnSiguiente");

                    //    // Ingreso de Clave Fiscal
                    //    await page.FillAsync("#F1\\:password", claveFiscal);
                    //    await page.ClickAsync("#F1\\:btnIngresar");

                    //    // Esperar a que cargue el panel principal (Home)
                    //    await page.WaitForURLAsync("**/zonaPrivada.xhtml**");
                    //    Console.WriteLine("¡Login exitoso!");

                    //    Console.WriteLine("2. Buscando el servicio 'Mis Comprobantes'...");
                    //    // Usamos un selector robusto basado en el texto del servicio
                    //    var servicioMisComprobantes = page.Locator("text=Mis Comprobantes");
                    //    await servicioMisComprobantes.ScrollIntoViewIfNeededAsync();

                    //    // ARCA abre los servicios en pestañas nuevas. Capturamos la nueva pestaña al hacer clic:
                    //    var waitForTargetTask = context.WaitForPageAsync();
                    //    await servicioMisComprobantes.ClickAsync();
                    //    var nuevaPestana = await waitForTargetTask;

                    //    // Esperar que la nueva pestaña cargue completamente
                    //    await nuevaPestana.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    //    Console.WriteLine("Accedido al módulo Mis Comprobantes.");

                    //    Console.WriteLine("3. Entrando a Comprobantes Recibidos...");
                    //    // ID interno del botón Recibidos en el portal
                    //    await nuevaPestana.ClickAsync("#btnRecibidos");
                    //    await nuevaPestana.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                    //    Console.WriteLine("4. Parametrizando el rango de fechas...");
                    //    // Llenar campos de fechas
                    //    await nuevaPestana.FillAsync("#fechaComprobanteFiltro", fechaDesde);
                    //    // Presionar Enter o Tab suele activar el formateador interno de la web de ARCA
                    //    await nuevaPestana.Keyboard.PressAsync("Tab");

                    //    // En ocasiones ARCA requiere hacer clic o limpiar antes de rellenar el hasta:
                    //    await nuevaPestana.FocusAsync("#fechaComprobanteHastaFiltro");
                    //    await nuevaPestana.FillAsync("#fechaComprobanteHastaFiltro", fechaHasta);
                    //    await nuevaPestana.Keyboard.PressAsync("Tab");

                    //    Console.WriteLine("5. Ejecutando la búsqueda...");
                    //    await nuevaPestana.ClickAsync("#btnBuscar");

                    //    // Esperar a que la tabla o los botones de exportación aparezcan
                    //    await nuevaPestana.WaitForSelectorAsync(".botones-exportar", new PageWaitForSelectorOptions { Timeout = 15000 });

                    //    Console.WriteLine("6. Descargando reporte en formato CSV / Excel...");
                    //    // Capturar el evento de descarga al hacer clic en el botón de Excel/CSV de la página
                    //    var waitForDownloadTask = nuevaPestana.WaitForDownloadAsync();

                    //    // Selector del botón "CSV" o "Excel" dentro de la botonera de ARCA
                    //    await nuevaPestana.ClickAsync("button.dt-button.buttons-csv");

                    //    var download = await waitForDownloadTask;

                    //    // Guardar el archivo en el directorio local
                    //    string rutaDestino = Path.Combine(Directory.GetCurrentDirectory(), $"comprobantes_recibidos_{cuit}.csv");
                    //    await download.SaveAsAsync(rutaDestino);

                    //    Console.WriteLine($"\n[ÉXITO] Archivo descargado correctamente en: {rutaDestino}");
                    //}
                    //catch (Exception ex)
                    //{
                    //    Console.WriteLine($"\n[ERROR] El proceso falló: {ex.Message}");
                    //}
                    //finally
                    //{
                    //    await context.CloseAsync();
                    //    await browser.CloseAsync();
                    //}
                    break;
            }
        }

        public Encoding GetEncoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "Encoding") == null)
                return Encoding.UTF8;
            if (int.TryParse(ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "Encoding")?.ivstrValue, out int livnum))
                return Encoding.GetEncoding(livnum);
            return Encoding.GetEncoding(ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "Encoding")?.ivstrValue);
        }
        public IDocsIO getIDocsIO()
        {
            IDocsIO liIDocsIO = null;
            eLoadMethod lioeLoadMethod;
            FileIO lioFileIO;
            lioeLoadMethod = (eLoadMethod)short.Parse(ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "LoadMethod")?.ivstrValue);
            switch (lioeLoadMethod)
            {
                case eLoadMethod.File:
                    lioFileIO = new FileIO(mioConfiguration);
                    lioFileIO.ivlngCuit = ioDcModel.ivlngCuit;
                    lioFileIO.ivstrPathIn = ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "InFolder")?.ivstrValue ?? string.Empty;
                    lioFileIO.ivstrPathOut = ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "OutFolder")?.ivstrValue ?? string.Empty;
                    lioFileIO.coFileExtensions = new string[] { ".txt" };
                    lioFileIO.ioMapper = ioCnfg.coServiceMappers.FirstOrDefault(x => x.ivstrWs == "rta");
                    liIDocsIO = lioFileIO;
                    break;
                case eLoadMethod.OracleCanonical:
                    OracleCanonical lioOracleCanonical = new OracleCanonical(mioConfiguration);
                    lioOracleCanonical.ivlngCuit = ioDcModel.ivlngCuit;
                    lioOracleCanonical.ivstrPathIn = ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "InFolder")?.ivstrValue ?? string.Empty;
                    lioOracleCanonical.ivstrPathOut = ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "OutFolder")?.ivstrValue ?? string.Empty;
                    lioOracleCanonical.ivstrUser = ioCnfg?.coParameters?.FirstOrDefault(x => x.ivstrId == "User")?.ivstrValue ?? string.Empty;
                    lioOracleCanonical.ivstrPass = ioCnfg?.coParameters?.FirstOrDefault(x => x.ivstrId == "Pass")?.ivstrValue ?? string.Empty;
                    lioOracleCanonical.ivstrEntityID = ioCnfg?.coParameters?.FirstOrDefault(x => x.ivstrId == "EntityID")?.ivstrValue ?? string.Empty;
                    lioOracleCanonical.ivnroDays = short.Parse(ioCnfg?.coParameters?.FirstOrDefault(x => x.ivstrId == "Days")?.ivstrValue ?? "0");
                    lioOracleCanonical.ivnroRows = short.Parse(ioCnfg?.coParameters?.FirstOrDefault(x => x.ivstrId == "Rows")?.ivstrValue ?? "0");
                    liIDocsIO = lioOracleCanonical;
                    break;
                case eLoadMethod.Api:
                case eLoadMethod.Manual:
                    ApiIO lioApiIO = new ApiIO(mioConfiguration, mioContext);
                    lioApiIO.ivlngCuit = ioDcModel.ivlngCuit;
                    lioApiIO.ioMapper = ioCnfg.coServiceMappers.FirstOrDefault(x => x.ivstrWs == "rta");
                    liIDocsIO = lioApiIO;
                    break;
                default:
                    break;
            }
            return liIDocsIO;
        }
        #endregion
        #region PRIVATE METHODS
        #endregion
    }
    public class CuitToLoadInfo
    {
        public Cuit? ioCuit { get; set; }
        public short ivnroLoadMethod { get; set; }
    }
}