using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pestaña "Ciudad": la ciudad 3D a pantalla completa con la interfaz encima (nivel, monedas, reloj y
/// las cinco áreas), y los menús del modo construir y de "ir a un edificio".
/// </summary>
public class PantallaCiudad : Pantalla
{
    private readonly Text nivel, monedas, reloj, estado;
    private readonly Image barraXp;
    private readonly Image[] rellenos = new Image[5];
    private readonly RectTransform hoja;
    private readonly RectTransform contenidoHoja;
    private int edificioElegido = -1;

    public PantallaCiudad(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla ciudad", 0, true)
    {
        // Cabecera: nivel, XP, reloj y monedas
        RectTransform filaCab = UIFabrica.Fila(columna, "Cabecera", 120);
        Image cab = Tarjeta(filaCab);
        Image retrato = UIFabrica.Caja(cab.transform, "Retrato", Paleta.Hex("2FA3A0"));
        retrato.sprite = UIFabrica.Circulo; retrato.type = Image.Type.Simple;
        retrato.rectTransform.anchorMin = retrato.rectTransform.anchorMax = new Vector2(0, 0.5f);
        retrato.rectTransform.sizeDelta = new Vector2(90, 90);
        retrato.rectTransform.anchoredPosition = new Vector2(70, 0);
        nivel = UIFabrica.Texto(cab.transform, "", 34, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(nivel.rectTransform, 0.15f, 0.5f, 0.45f, 0.95f);
        Image fondoXp = UIFabrica.Caja(cab.transform, "Fondo XP", Paleta.Gris);
        UIFabrica.Anclar(fondoXp.rectTransform, 0.15f, 0.2f, 0.42f, 0.38f);
        barraXp = UIFabrica.Caja(fondoXp.transform, "XP", Paleta.Hex("E3B341"));
        barraXp.type = Image.Type.Filled; barraXp.fillMethod = Image.FillMethod.Horizontal; barraXp.sprite = UIFabrica.Plano;
        UIFabrica.Anclar(barraXp.rectTransform, 0, 0, 1, 1);
        reloj = UIFabrica.Texto(cab.transform, "", 40, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(reloj.rectTransform, 0.44f, 0, 0.72f, 1);
        monedas = UIFabrica.Texto(cab.transform, "", 34, Paleta.Texto, TextAnchor.MiddleRight, FontStyle.Bold);
        UIFabrica.Anclar(monedas.rectTransform, 0.7f, 0, 0.96f, 1);

        // Las cinco áreas (tocar abre "Tu equilibrio")
        RectTransform filaAreas = UIFabrica.Fila(columna, "Áreas", 86);
        Button areas = UIFabrica.Boton(filaAreas, "", Paleta.Tarjeta, Paleta.Texto, 10, () => this.juego.IrAEquilibrio());
        UIFabrica.Anclar((RectTransform)areas.transform, 0, 0, 1, 1);
        foreach (Area area in Areas.Todas)
        {
            int i = (int)area;
            float x0 = i / 5f + 0.015f, x1 = (i + 1) / 5f - 0.015f;
            Text n = UIFabrica.Texto(areas.transform, Areas.Nombre(area), 22, Paleta.DeArea(area), TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFabrica.Anclar(n.rectTransform, x0, 0.5f, x1, 0.95f);
            Image fondo = UIFabrica.Caja(areas.transform, "Fondo " + i, Paleta.Gris);
            UIFabrica.Anclar(fondo.rectTransform, x0, 0.15f, x1, 0.42f);
            rellenos[i] = UIFabrica.Caja(fondo.transform, "Relleno", Paleta.DeArea(area));
            rellenos[i].type = Image.Type.Filled; rellenos[i].fillMethod = Image.FillMethod.Horizontal; rellenos[i].sprite = UIFabrica.Plano;
            UIFabrica.Anclar(rellenos[i].rectTransform, 0, 0, 1, 1);
        }

        RectTransform filaAyuda = UIFabrica.Fila(columna, "Ayuda", 56);
        Image fondoAyuda = UIFabrica.Caja(filaAyuda, "Fondo", new Color(1, 1, 1, 0.75f));
        UIFabrica.Anclar(fondoAyuda.rectTransform, 0, 0, 1, 1);
        fondoAyuda.raycastTarget = false;
        Text ayuda = UIFabrica.Texto(fondoAyuda.transform, "Toca un edificio: si tiene \"?\", decide qué es; si ya tiene papel, ve allí.", 24, Paleta.Texto, TextAnchor.MiddleCenter);
        UIFabrica.Anclar(ayuda.rectTransform, 0.02f, 0, 0.98f, 1);

        UIFabrica.Relleno(columna);
        RectTransform filaEstado = UIFabrica.Fila(columna, "Estado", 60);
        estado = UIFabrica.Texto(filaEstado, "", 32, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(estado.rectTransform, 0, 0, 1, 1);

        // Hoja inferior para los menús (fuera de la columna)
        Image fondoHoja = UIFabrica.Caja(raizRT, "Hoja", Paleta.Fondo);
        hoja = fondoHoja.rectTransform;
        hoja.anchorMin = new Vector2(0, 0);
        hoja.anchorMax = new Vector2(1, 0);
        hoja.pivot = new Vector2(0.5f, 0);
        hoja.sizeDelta = new Vector2(0, 760);
        hoja.anchoredPosition = new Vector2(0, AltoBarra);
        contenidoHoja = UIFabrica.Columna(hoja, "Contenido", 40, 16);
        hoja.gameObject.SetActive(false);
    }

    protected override void AlMostrar()
    {
        CerrarHoja();
    }

    public override void Actualizar()
    {
        Progreso pr = juego.Progreso;
        nivel.text = "Nv. " + pr.Nivel;
        barraXp.fillAmount = pr.XpEnNivel / (float)Progreso.XpPorNivel;
        reloj.text = "Día " + juego.Dia + " · " + Jornada.Hora(juego.MinutoEntero);
        monedas.text = pr.Datos.monedas + " monedas";
        foreach (Area area in Areas.Todas) rellenos[(int)area].fillAmount = juego.Jornada.Estado[area] / 100f;
        if (juego.Ciudad.Caminando) estado.text = "Caminando...";
        else if (!hoja.gameObject.activeSelf) estado.text = "";
    }

    // ---------------------------------------------------------------- menús

    public void AlTocarEdificio(int indice)
    {
        edificioElegido = indice;
        RolEdificio rol = Catalogos.BuscarRol(juego.Datos.ciudad.roles[indice]);
        if (rol == null) AbrirConstruir();
        else AbrirIr(rol);
    }

    private void LimpiarHoja()
    {
        for (int i = contenidoHoja.childCount - 1; i >= 0; i--) Object.Destroy(contenidoHoja.GetChild(i).gameObject);
        hoja.gameObject.SetActive(true);
        juego.Ciudad.Activa = false;
    }

    public void CerrarHoja()
    {
        hoja.gameObject.SetActive(false);
        juego.Ciudad.Activa = true;
    }

    private void Titulo(string texto)
    {
        Text t = UIFabrica.Texto(UIFabrica.Fila(contenidoHoja, "Título", 80), texto, 46, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(t.rectTransform, 0, 0, 1, 1);
    }

    private Button Boton(Transform padre, string texto, Color fondo, Color colorTexto, System.Action alPulsar)
    {
        return UIFabrica.Boton(padre, texto, fondo, colorTexto, 32, alPulsar);
    }

    private void AbrirConstruir()
    {
        LimpiarHoja();
        Titulo("Modo construir: ¿qué es este edificio?");
        RolEdificio[] roles = Catalogos.Roles;
        for (int f = 0; f < 4; f++)
        {
            RectTransform fila = UIFabrica.Fila(contenidoHoja, "Roles " + f, 100);
            for (int c = 0; c < 2; c++)
            {
                int i = f * 2 + c;
                if (i >= roles.Length) break;
                RolEdificio rol = roles[i];
                Button b = Boton(fila, rol.nombre, Color.Lerp(Paleta.DeArea(rol.area), Color.white, 0.45f), Paleta.Texto, () => AsignarRol(rol.id));
                UIFabrica.Anclar((RectTransform)b.transform, c * 0.5f + 0.01f, 0, (c + 1) * 0.5f - 0.01f, 1);
            }
        }
        RectTransform pie = UIFabrica.Fila(contenidoHoja, "Pie", 100);
        Button cancelar = Boton(pie, "Cancelar", Paleta.Gris, Paleta.Texto, CerrarHoja);
        UIFabrica.Anclar((RectTransform)cancelar.transform, 0, 0, 0.49f, 1);
        if (!string.IsNullOrEmpty(juego.Datos.ciudad.roles[edificioElegido]) && edificioElegido != Ciudad3D.IndiceCasa)
        {
            Button quitar = Boton(pie, "Quitar papel", Paleta.TramoDemasiado, Paleta.Texto, () => AsignarRol(""));
            UIFabrica.Anclar((RectTransform)quitar.transform, 0.51f, 0, 1, 1);
        }
    }

    private void AsignarRol(string rol)
    {
        juego.AsignarRol(edificioElegido, rol);
        CerrarHoja();
    }

    private void AbrirIr(RolEdificio rol)
    {
        LimpiarHoja();
        Titulo(rol.nombre + ": ¿qué vas a hacer aquí?");
        foreach (string id in rol.acciones)
        {
            DefinicionAccion a = juego.Tabla.Buscar(id);
            RectTransform fila = UIFabrica.Fila(contenidoHoja, a.nombre, 96);
            Button b = Boton(fila, "Ir y empezar: " + a.nombre + "  (zona óptima " + PantallaHacer.Rango(a) + ")", Paleta.Coral, Paleta.Blanco,
                () => Ir(a));
            UIFabrica.Anclar((RectTransform)b.transform, 0, 0, 1, 1);
        }
        RectTransform pie = UIFabrica.Fila(contenidoHoja, "Pie", 96);
        Button soloIr = Boton(pie, "Solo ir", Paleta.Tarjeta, Paleta.Texto, () => Ir(null));
        Button cambiar = Boton(pie, "Cambiar papel", Paleta.Gris, Paleta.Texto, AbrirConstruir);
        Button cerrar = Boton(pie, "Cerrar", Paleta.Gris, Paleta.Texto, CerrarHoja);
        UIFabrica.Anclar((RectTransform)soloIr.transform, 0f, 0f, 0.32f, 1f);
        UIFabrica.Anclar((RectTransform)cambiar.transform, 0.34f, 0f, 0.66f, 1f);
        UIFabrica.Anclar((RectTransform)cerrar.transform, 0.68f, 0f, 1f, 1f);
        if (edificioElegido == Ciudad3D.IndiceCasa) cambiar.gameObject.SetActive(false);
    }

    private void Ir(DefinicionAccion accion)
    {
        CerrarHoja();
        int destino = edificioElegido;
        RolEdificio rol = Catalogos.BuscarRol(juego.Datos.ciudad.roles[destino]);
        estado.text = "Caminando a " + (rol != null ? rol.nombre : "ese edificio") + "...";
        juego.Ciudad.IrA(destino, () =>
        {
            estado.text = "";
            if (accion != null) juego.EmpezarAccion(accion);
        });
    }
}
