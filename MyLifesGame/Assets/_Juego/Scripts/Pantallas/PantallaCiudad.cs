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
    private int nivelBarrios = -1;
    private readonly RectTransform guia;
    private readonly Image fondoAyuda;
    private readonly Image pildoraZoom;
    private RawImage cara;

    public PantallaCiudad(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla ciudad", 0, true)
    {
        // Cabecera compacta como en la imagen de Flow: píldora con retrato redondo, nivel y XP a la izquierda;
        // monedas y ajustes a la derecha; el reloj del día, pequeño, debajo de las monedas.
        RectTransform filaCab = UIFabrica.Fila(columna, "Cabecera", 112);
        Image pildoraNivel = UIFabrica.Caja(filaCab, "Nivel", Paleta.Tarjeta);
        UIFabrica.Anclar(pildoraNivel.rectTransform, 0.12f, 0.2f, 0.5f, 0.8f);
        UIFabrica.Sombra(pildoraNivel, 4f, 0.15f);
        Image aro = UIFabrica.Caja(filaCab, "Aro", Paleta.Blanco);
        aro.sprite = UIFabrica.Circulo; aro.type = Image.Type.Simple;
        aro.rectTransform.anchorMin = aro.rectTransform.anchorMax = new Vector2(0, 0.5f);
        aro.rectTransform.sizeDelta = new Vector2(112, 112);
        aro.rectTransform.anchoredPosition = new Vector2(64, 0);
        UIFabrica.Sombra(aro, 4f, 0.18f);
        Image retrato = UIFabrica.Caja(aro.transform, "Retrato", Paleta.Hex("FFE7C2"));
        retrato.sprite = UIFabrica.Circulo; retrato.type = Image.Type.Simple;
        UIFabrica.Anclar(retrato.rectTransform, 0.06f, 0.06f, 0.94f, 0.94f);
        retrato.gameObject.AddComponent<Mask>().showMaskGraphic = true;
        var cara = new GameObject("Cara 3D", typeof(RectTransform)).AddComponent<RawImage>();
        cara.transform.SetParent(retrato.transform, false);
        cara.texture = EstudioAvatar.Textura;
        cara.uvRect = new Rect(0.22f, 0.5f, 0.56f, 0.45f);
        this.cara = cara;
        cara.raycastTarget = false;
        UIFabrica.Anclar(cara.rectTransform, 0, 0, 1, 1);
        nivel = UIFabrica.Texto(pildoraNivel.transform, "", 30, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(nivel.rectTransform, 0.36f, 0.48f, 0.97f, 0.98f);
        Image fondoXp = UIFabrica.Caja(pildoraNivel.transform, "Fondo XP", Paleta.Gris);
        UIFabrica.Anclar(fondoXp.rectTransform, 0.36f, 0.2f, 0.9f, 0.4f);
        barraXp = UIFabrica.Caja(fondoXp.transform, "XP", Paleta.Hex("5BC0B5"));
        barraXp.type = Image.Type.Filled; barraXp.fillMethod = Image.FillMethod.Horizontal; barraXp.sprite = UIFabrica.Plano;
        UIFabrica.Anclar(barraXp.rectTransform, 0, 0, 1, 1);

        Image pildora = UIFabrica.Caja(filaCab, "Monedas", Paleta.Tarjeta);
        UIFabrica.Anclar(pildora.rectTransform, 0.62f, 0.38f, 0.86f, 0.92f);
        UIFabrica.Sombra(pildora, 4f, 0.15f);
        Image moneda = UIFabrica.Caja(pildora.transform, "Moneda", Paleta.Hex("E3B341"));
        moneda.sprite = UIFabrica.Circulo; moneda.type = Image.Type.Simple;
        moneda.rectTransform.anchorMin = moneda.rectTransform.anchorMax = new Vector2(0, 0.5f);
        moneda.rectTransform.sizeDelta = new Vector2(42, 42);
        moneda.rectTransform.anchoredPosition = new Vector2(32, 0);
        Image brilloMoneda = UIFabrica.Caja(moneda.transform, "Brillo", Paleta.Hex("F7D774"));
        brilloMoneda.sprite = UIFabrica.Circulo; brilloMoneda.type = Image.Type.Simple;
        UIFabrica.Anclar(brilloMoneda.rectTransform, 0.2f, 0.2f, 0.8f, 0.8f);
        monedas = UIFabrica.Texto(pildora.transform, "", 30, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(monedas.rectTransform, 0.32f, 0, 0.96f, 1);
        reloj = UIFabrica.Texto(filaCab, "", 22, Paleta.TextoSuave, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(reloj.rectTransform, 0.58f, 0, 0.9f, 0.36f);

        Button ajustes = UIFabrica.Boton(filaCab, "", Paleta.Tarjeta, Paleta.Texto, 10, () => this.juego.IrAPestana(3));
        RectTransform ra = (RectTransform)ajustes.transform;
        ra.anchorMin = ra.anchorMax = new Vector2(1, 0.65f);
        ra.sizeDelta = new Vector2(76, 76);
        ra.anchoredPosition = new Vector2(-46, 0);
        Image fa = ajustes.GetComponent<Image>();
        if (fa != null) { fa.sprite = UIFabrica.Circulo; fa.type = Image.Type.Simple; UIFabrica.Sombra(fa, 4f, 0.15f); }
        RectTransform zg = UIFabrica.Crear("Engranaje", ra);
        UIFabrica.Anclar(zg, 0.18f, 0.18f, 0.82f, 0.82f);
        FormasUI engranaje = FormasUI.En(zg.gameObject);
        engranaje.raycastTarget = false;
        Engranaje(engranaje, Paleta.Texto);

        // Las cinco áreas en una píldora blanca: círculos de color con su icono (el anillo muestra el valor).
        // Tocar abre "Tu equilibrio".
        RectTransform filaAreas = UIFabrica.Fila(columna, "Áreas", 124);
        Button areas = UIFabrica.Boton(filaAreas, "", Paleta.Tarjeta, Paleta.Texto, 10, () => this.juego.IrAEquilibrio());
        UIFabrica.Anclar((RectTransform)areas.transform, 0.1f, 0, 0.9f, 1);
        Image fondoAreas = areas.GetComponent<Image>();
        if (fondoAreas != null) UIFabrica.Sombra(fondoAreas, 4f, 0.15f);
        foreach (Area area in Areas.Todas)
        {
            int i = (int)area;
            RectTransform hueco = UIFabrica.Crear("Medidor " + i, areas.transform);
            hueco.anchorMin = hueco.anchorMax = new Vector2((i + 0.5f) / 5f, 0.6f);
            hueco.sizeDelta = new Vector2(78, 78);
            Image fondoAnillo = UIFabrica.Caja(hueco, "Fondo", Paleta.Gris);
            fondoAnillo.sprite = UIFabrica.Circulo; fondoAnillo.type = Image.Type.Simple;
            UIFabrica.Anclar(fondoAnillo.rectTransform, 0, 0, 1, 1);
            rellenos[i] = UIFabrica.Caja(hueco, "Anillo", Color.Lerp(Paleta.DeArea(area), Color.black, 0.15f));
            rellenos[i].sprite = UIFabrica.Circulo;
            rellenos[i].type = Image.Type.Filled;
            rellenos[i].fillMethod = Image.FillMethod.Radial360;
            rellenos[i].fillOrigin = (int)Image.Origin360.Top;
            UIFabrica.Anclar(rellenos[i].rectTransform, 0, 0, 1, 1);
            Image centro = UIFabrica.Caja(hueco, "Centro", Paleta.DeArea(area));
            centro.sprite = UIFabrica.Circulo; centro.type = Image.Type.Simple;
            UIFabrica.Anclar(centro.rectTransform, 0.1f, 0.1f, 0.9f, 0.9f);
            FormasUI icono = Iconos.Crear(hueco, area, 58f);
            icono.rectTransform.anchorMin = icono.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            Text n = UIFabrica.Texto(areas.transform, Areas.Nombre(area), 20, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
            n.rectTransform.anchorMin = n.rectTransform.anchorMax = new Vector2((i + 0.5f) / 5f, 0.13f);
            n.rectTransform.sizeDelta = new Vector2(120, 30);
        }

        UIFabrica.Relleno(columna);
        RectTransform filaAyuda = UIFabrica.Fila(columna, "Ayuda", 56);
        fondoAyuda = UIFabrica.Caja(filaAyuda, "Fondo", new Color(1, 1, 1, 0.75f));
        UIFabrica.Anclar(fondoAyuda.rectTransform, 0, 0, 1, 1);
        fondoAyuda.raycastTarget = false;
        Text ayuda = UIFabrica.Texto(fondoAyuda.transform, "Toca un edificio: si tiene \"?\", decide qué es; si ya tiene papel, ve allí.", 24, Paleta.Texto, TextAnchor.MiddleCenter);
        UIFabrica.Anclar(ayuda.rectTransform, 0.02f, 0, 0.98f, 1);

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

        // Guía de la primera vez: una mano que señala el botón "Hacer"
        guia = UIFabrica.Crear("Guía", raizRT);
        guia.anchorMin = guia.anchorMax = new Vector2(0.5f, 0f);
        guia.pivot = new Vector2(0.5f, 0f);
        guia.sizeDelta = new Vector2(600, 330);
        Image globo = UIFabrica.Caja(guia, "Globo", Paleta.Blanco);
        UIFabrica.Anclar(globo.rectTransform, 0, 0.56f, 1, 1);
        UIFabrica.Sombra(globo, 6f, 0.2f);
        globo.raycastTarget = false;
        Text tg = UIFabrica.Texto(globo.transform, "¡Tu primera acción! Toca «Hacer» y elige qué vas a hacer de verdad.", 30, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(tg.rectTransform, 0.04f, 0, 0.96f, 1);
        RectTransform zm = UIFabrica.Crear("Mano", guia);
        zm.anchorMin = zm.anchorMax = new Vector2(0.5f, 0f);
        zm.pivot = new Vector2(0.5f, 0f);
        zm.sizeDelta = new Vector2(150, 150);
        FormasUI mano = FormasUI.En(zm.gameObject);
        mano.raycastTarget = false;
        Iconos.Mano(mano);
        guia.gameObject.SetActive(false);

        // Botones de zoom (+ / −) a la derecha, como en la referencia; en el móvil también vale pellizcar
        pildoraZoom = UIFabrica.Caja(raizRT, "Zoom", Paleta.Tarjeta);
        RectTransform rz = pildoraZoom.rectTransform;
        rz.anchorMin = rz.anchorMax = new Vector2(1, 0);
        rz.pivot = new Vector2(1, 0);
        rz.sizeDelta = new Vector2(92, 190);
        rz.anchoredPosition = new Vector2(-28, AltoBarra + 60);
        UIFabrica.Sombra(pildoraZoom, 4f, 0.15f);
        Button mas = UIFabrica.Boton(rz, "+", Paleta.Tarjeta, Paleta.Texto, 52, () => this.juego.Ciudad.CambiarZoom(0.8f));
        UIFabrica.Anclar((RectTransform)mas.transform, 0.06f, 0.52f, 0.94f, 0.97f);
        Button menos = UIFabrica.Boton(rz, "\u2212", Paleta.Tarjeta, Paleta.Texto, 52, () => this.juego.Ciudad.CambiarZoom(1.25f));
        UIFabrica.Anclar((RectTransform)menos.transform, 0.06f, 0.03f, 0.94f, 0.48f);
    }

    /// <summary>Engranaje de ajustes dibujado con formas (dientes, aro y agujero).</summary>
    private static void Engranaje(FormasUI f, Color c)
    {
        f.Limpiar();
        for (int i = 0; i < 4; i++) f.Rect(Vector2.zero, new Vector2(0.34f, 1.75f), i * 45f, c);
        f.Circulo(Vector2.zero, 0.66f, c);
        f.Circulo(Vector2.zero, 0.28f, Paleta.Tarjeta);
    }

    public bool GuiaVisible { get { return guia.gameObject.activeSelf; } }
    public bool HojaAbierta { get { return hoja.gameObject.activeSelf; } }
    public float AltoHoja { get { return hoja.sizeDelta.y; } }

    protected override void AlMostrar()
    {
        CerrarHoja();
    }

    public override void Actualizar()
    {
        Progreso pr = juego.Progreso;
        pildoraZoom.gameObject.SetActive(!HojaAbierta && !GuiaVisible);
        // El personaje generado tiene la cabeza a tamaño normal (no chibi): se encuadra más arriba y más cerca
        if (cara != null) cara.uvRect = Avatar3D.UsaGenerado ? new Rect(0.35f, 0.7f, 0.3f, 0.26f) : new Rect(0.22f, 0.5f, 0.56f, 0.45f);
        nivel.text = "Nv. " + pr.Nivel;
        if (pr.Nivel != nivelBarrios) { nivelBarrios = pr.Nivel; juego.Ciudad.ActualizarBarrios(pr.Nivel); }
        barraXp.fillAmount = pr.XpEnNivel / (float)Progreso.XpPorNivel;
        reloj.text = "Día " + juego.Dia + " · " + Jornada.Hora(juego.MinutoEntero);
        monedas.text = pr.Datos.monedas.ToString();
        foreach (Area area in Areas.Todas) rellenos[(int)area].fillAmount = juego.Jornada.Estado[area] / 100f;
        bool verGuia = !juego.Datos.guiaHacerHecha && !HojaAbierta && !juego.Ciudad.Caminando;
        if (guia.gameObject.activeSelf != verGuia) { guia.gameObject.SetActive(verGuia); guia.SetAsLastSibling(); fondoAyuda.gameObject.SetActive(!verGuia); }
        if (verGuia) guia.anchoredPosition = new Vector2(0, AltoBarra + 10f + Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4f)) * 26f);
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

    private void LimpiarHoja(float alto)
    {
        juego.Ciudad.SalirConstruir();
        hoja.sizeDelta = new Vector2(0, alto);
        for (int i = contenidoHoja.childCount - 1; i >= 0; i--) Object.Destroy(contenidoHoja.GetChild(i).gameObject);
        hoja.gameObject.SetActive(true);
        juego.Ciudad.Activa = false;
    }

    public void CerrarHoja()
    {
        hoja.gameObject.SetActive(false);
        if (juego.Ciudad != null) juego.Ciudad.SalirConstruir();
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
        LimpiarHoja(850);
        juego.Ciudad.ModoConstruir(edificioElegido);
        int manzanas = Ciudad3D.Manzanas(edificioElegido);
        Titulo("Modo construir: ¿qué es este edificio?");
        Text dist = UIFabrica.Texto(UIFabrica.Fila(contenidoHoja, "Distancia", 40),
            manzanas == 0 ? "Es tu casa." : "A " + manzanas + (manzanas == 1 ? " manzana" : " manzanas") + " de casa (unos " + manzanas * 2 + " min andando). Lo cercano se usa más.",
            26, Paleta.TextoSuave, TextAnchor.MiddleLeft);
        UIFabrica.Anclar(dist.rectTransform, 0, 0, 1, 1);
        RolEdificio[] roles = Catalogos.Roles;
        for (int f = 0; f < 4; f++)
        {
            RectTransform fila = UIFabrica.Fila(contenidoHoja, "Roles " + f, 104);
            for (int c = 0; c < 2; c++)
            {
                int i = f * 2 + c;
                if (i >= roles.Length) break;
                RolEdificio rol = roles[i];
                bool actual = juego.Datos.ciudad.roles[edificioElegido] == rol.id;
                Button b = Boton(fila, rol.nombre, actual ? Color.Lerp(Paleta.DeArea(rol.area), Color.white, 0.55f) : Paleta.Tarjeta, Paleta.Texto, () => AsignarRol(rol.id));
                UIFabrica.Anclar((RectTransform)b.transform, c * 0.5f + 0.01f, 0, (c + 1) * 0.5f - 0.01f, 1);
                Text t = b.GetComponentInChildren<Text>();
                t.alignment = TextAnchor.MiddleLeft;
                UIFabrica.Anclar(t.rectTransform, 0.3f, 0, 0.97f, 1);
                FormasUI ic = Iconos.Crear(b.transform, rol.area, 64f);
                ic.rectTransform.anchorMin = ic.rectTransform.anchorMax = new Vector2(0, 0.5f);
                ic.rectTransform.anchoredPosition = new Vector2(56, 0);
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
        LimpiarHoja(120 + 96 + 16 + rol.acciones.Length * 112 + 112);
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
