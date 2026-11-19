namespace MyLifesGame.Motor
{
    /// <summary>Una prenda que se puede comprar para el avatar.</summary>
    public class Prenda
    {
        public string id;
        public string nombre;
        public string colorHex;
        public int precio;
        public int nivelMinimo;
    }

    /// <summary>Un logro del Álbum.</summary>
    public class Logro
    {
        public string id;
        public string nombre;
        public string descripcion;
        public Area area;
    }

    /// <summary>El papel que el jugador da a un edificio de su ciudad (modo construir).</summary>
    public class RolEdificio
    {
        public string id;
        public string nombre;
        public Area area;           // color del pin
        public string[] acciones;   // acciones que se hacen allí
    }

    /// <summary>Plantilla de misión del día.</summary>
    public class PlantillaMision
    {
        public string accionId;
        public int minutos;
        public string titulo;
        public Area area;
        public int xp;
    }

    public static class Catalogos
    {
        // Monedas provisionales: las recompensas están aparcadas en el diseño (ver docs, sección 5).
        public static readonly Prenda[] Prendas =
        {
            new Prenda { id = "teal", nombre = "Sudadera turquesa", colorHex = "2FA3A0", precio = 0, nivelMinimo = 1 },
            new Prenda { id = "coral", nombre = "Sudadera coral", colorHex = "E8604C", precio = 60, nivelMinimo = 1 },
            new Prenda { id = "mostaza", nombre = "Sudadera mostaza", colorHex = "E3A72F", precio = 60, nivelMinimo = 1 },
            new Prenda { id = "lavanda", nombre = "Sudadera lavanda", colorHex = "A98BD9", precio = 90, nivelMinimo = 2 },
            new Prenda { id = "bosque", nombre = "Sudadera verde bosque", colorHex = "3E7D52", precio = 120, nivelMinimo = 3 },
            new Prenda { id = "noche", nombre = "Sudadera azul noche", colorHex = "2F3E6B", precio = 150, nivelMinimo = 4 },
        };

        public static readonly Logro[] Logros =
        {
            new Logro { id = "primera_accion", nombre = "Primer paso", descripcion = "Termina tu primera acción", area = Area.Animo },
            new Logro { id = "dosis_optima", nombre = "En su punto", descripcion = "Termina una acción en zona óptima", area = Area.Energia },
            new Logro { id = "constancia", nombre = "Constancia", descripcion = "3 acciones en zona óptima", area = Area.Fisico },
            new Logro { id = "hidratado", nombre = "Hidratado", descripcion = "Llega a tu meta de agua", area = Area.Energia },
            new Logro { id = "socializador", nombre = "Socializador", descripcion = "Queda con amigos 1 h o más", area = Area.Social },
            new Logro { id = "equilibrio", nombre = "Equilibrio total", descripcion = "Las cinco áreas en zona buena o alta", area = Area.Mente },
            new Logro { id = "constructor", nombre = "Constructor", descripcion = "Da un papel a 3 edificios de tu ciudad", area = Area.Social },
            new Logro { id = "estilo", nombre = "Estilo propio", descripcion = "Compra tu primera sudadera", area = Area.Animo },
            new Logro { id = "paseante", nombre = "Paseante", descripcion = "6.000 pasos en un día", area = Area.Fisico },
            new Logro { id = "mision", nombre = "Misión cumplida", descripcion = "Completa una misión del día", area = Area.Mente },
        };

        public static readonly RolEdificio[] Roles =
        {
            new RolEdificio { id = "casa", nombre = "Casa", area = Area.Animo, acciones = new[] { "descansar", "meditar", "videojuegos", "estudiar" } },
            new RolEdificio { id = "gimnasio", nombre = "Gimnasio", area = Area.Fisico, acciones = new[] { "entrenar" } },
            new RolEdificio { id = "cafeteria", nombre = "Cafetería", area = Area.Social, acciones = new[] { "quedar" } },
            new RolEdificio { id = "biblioteca", nombre = "Biblioteca", area = Area.Mente, acciones = new[] { "estudiar" } },
            new RolEdificio { id = "trabajo", nombre = "Trabajo", area = Area.Mente, acciones = new[] { "trabajar" } },
            new RolEdificio { id = "parque", nombre = "Parque", area = Area.Energia, acciones = new[] { "pasear", "meditar" } },
            new RolEdificio { id = "amigos", nombre = "Casa de amigos", area = Area.Social, acciones = new[] { "quedar", "videojuegos" } },
            new RolEdificio { id = "recreativos", nombre = "Recreativos", area = Area.Animo, acciones = new[] { "videojuegos" } },
        };

        public static readonly PlantillaMision[] Misiones =
        {
            new PlantillaMision { accionId = "entrenar", minutos = 30, titulo = "Entrenar 30 minutos", area = Area.Fisico, xp = 50 },
            new PlantillaMision { accionId = "pasear", minutos = 20, titulo = "Pasear 20 minutos", area = Area.Energia, xp = 20 },
            new PlantillaMision { accionId = "estudiar", minutos = 45, titulo = "Estudiar 45 minutos", area = Area.Mente, xp = 30 },
            new PlantillaMision { accionId = "quedar", minutos = 30, titulo = "Quedar con alguien", area = Area.Social, xp = 40 },
            new PlantillaMision { accionId = "meditar", minutos = 10, titulo = "Meditar 10 minutos", area = Area.Animo, xp = 15 },
            new PlantillaMision { accionId = "descansar", minutos = 20, titulo = "Descansar 20 minutos", area = Area.Energia, xp = 15 },
        };

        public static Prenda BuscarPrenda(string id)
        {
            foreach (Prenda p in Prendas) if (p.id == id) return p;
            return Prendas[0];
        }

        public static RolEdificio BuscarRol(string id)
        {
            foreach (RolEdificio r in Roles) if (r.id == id) return r;
            return null;
        }

        public static Logro BuscarLogro(string id)
        {
            foreach (Logro l in Logros) if (l.id == id) return l;
            return null;
        }
    }
}
