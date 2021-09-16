namespace WebService.Models
{
    public class MuestraMinsal
    {
        public string codigo_muestra_cliente { get; set; }
        public string epivigila { get; set; }
        public long id_laboratorio { get; set; }
        public string rut_responsable { get; set; }
        public string paciente_tipodoc { get; set; }
        public string paciente_nombres { get; set; }
        public string paciente_ap_pat { get; set; }
        public string paciente_ap_mat { get; set; }
        public string paciente_fecha_nac { get; set; }
        public string paciente_comuna { get; set; }
        public string paciente_direccion { get; set; }
        public string paciente_telefono { get; set; }
        public string paciente_sexo { get; set; }
        public string cod_deis { get; set; }
        public string fecha_muestra { get; set; }
        public string tecnica_muestra { get; set; }
        public string tipo_muestra { get; set; }
        public string paciente_run { get; set; }
        public string paciente_dv { get; set; }
        public string paciente_prevision { get; set; }
        public string paciente_pasaporte { get; set; }
        public int paciente_ext_paisorigen { get; set; }
        public bool? busqueda_activa { get; set; }
    }
}
