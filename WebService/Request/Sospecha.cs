using System;

namespace WebService.Request
{
    
    /// <summary>
    /// Representa el caso de sospecha en el monitor esmeralda
    /// </summary>
    public class Sospecha
    {
        /// <summary>
        /// Identificador del caso de sospecha
        /// </summary>
        public long? id { get; set; }
        
        /// <summary>
        /// Edad del paciente al momento de crear el caso de sospecha.
        /// </summary>
        public int? age { get; set; }
        
        /// <summary>
        /// Genero del paciente. Los valores que puede tener son: male, female, other, unknown
        /// </summary>
        public string gender { get; set; }
        
        /// <summary>
        /// Fecha y hora en que la muestra del caso fue tomada.
        /// El formato de la fecha de estar en el estándar ISO 8601
        /// </summary>
        public DateTime? sample_at { get; set; }
        
        /// <summary>
        /// Número de la semana epidemiológica en que se creo el caso de sospecha.
        /// </summary>
        public int? epidemiological_week { get; set; }
        
        public string origin { get; set; }
        public string status { get; set; }
        
        /// <summary>
        /// RUN del medico que solicitó la evaluación.
        /// </summary>
        public string run_medic { get; set; }
        
        /// <summary>
        /// Indica si tuvo o no tuvo síntomas
        /// </summary>
        /// <remarks>
        /// Los valores pueden ser: Si y No
        /// </remarks>
        public string symptoms { get; set; }
        
        /// <summary>
        /// Fecha que indica el paciente que comenzaron sus síntomas.
        /// El formato de la fecha debe estar en el estándar ISO 8601
        /// </summary>
        public DateTime? symptoms_at { get; set; }
        
        /// <summary>
        /// Fecha y hora en que se recepcionó la muestra por parte del laboratorio. 
        /// El formato de la fecha debe estar en el estándar ISO 8601
        /// </summary>
        public DateTime? reception_at { get; set; }
        
        /// <summary>
        /// Identifica al usuario que recibió la muestra en el laboratorio.
        /// </summary>
        public long? receptor_id { get; set; }
        
        //public DateTime? result_ifd_at { get; set; }
        //public string result_ifd { get; set; }
        //public string subtype { get; set; }
        
        /// <summary>
        /// Fecha y hora en que se entregó el resultado del laboratorio.
        /// El formato de la fecha debe estar en el estándar ISO 8601
        /// </summary>
        public DateTime? pscr_sars_cov_2_at { get; set; }
        
        /// <summary>
        /// Indica el resultado del muestra para el caso de sospecha.  
        /// Los valores que puede tener son los siguientes:
        /// pending, negative, positive, rejected, undetermined
        /// </summary>
        public string pscr_sars_cov_2 { get; set; }
        
        /// <summary>
        /// Define el tipo de muestra que tiene asociado el caso de sospecha
        /// Sus valores pueden ser:
        ///   TÓRULAS NASOFARÍNGEAS,
        ///   ESPUTO,
        ///   TÓRULAS NASOFARÍNGEAS/ESPUTO,
        ///   ASPIRADO NASOFARÍNGEO,
        ///   LAVADO BRONCOALVEOLAR,
        ///   ASPIRADO TRAQUEAL,
        ///   MUESTRA SANGUÍNEA,
        ///   TEJIDO PULMONAR,
        ///   SALIVA,
        ///   OTRO
        /// </summary>
        public string sample_type { get; set; }
        
        /// <summary>
        /// Identifica el usuario que valida el resultado de la muestra
        /// </summary>
        public long? validator_id { get; set; }
        
        public DateTime? sent_isp_at { get; set; }
        public string external_laboratory { get; set; }
        public int? paho_flu { get; set; }
        
        /// <summary>
        /// Identificador en sistema de minsal EPIVIGILIA
        /// </summary>
        public int? epivigila { get; set; }
        
        /// <summary>
        /// Indica si la paciente esta embarazada o no
        /// </summary>
        public bool? gestation { get; set; }
        
        /// <summary>
        /// Semana de gestación que se encuentra la paciente.
        /// </summary>
        public int? gestation_week { get; set; }
        
        /// <summary>
        /// Indica si el paciente fue debido a un contacto estrecho.
        /// </summary>
        public bool? close_contact { get; set; }
        
        /// <summary>
        /// Indica si el paciente es funcionario de la salud.
        /// </summary>
        public bool? functionary { get; set; }
        
        //public DateTime? notification_at { get; set; }
        //public string notification_mechanism { get; set; }
        //public DateTime? discharged_at { get; set; }
        
        /// <summary>
        /// Observaciones adicionales al caso de sospecha.
        /// </summary>
        public string observation { get; set; }
        //public bool? discharge_test { get; set; }
        //public long? minsal_ws_id { get; set; }
        
        /// <summary>
        /// Identificador interno del paciente asociado al caso de sospecha.
        /// </summary>
        public long? patient_id { get; set; }
        
        /// <summary>
        /// Identificador del laboratorio que recepciona y analiza la muestra.
        /// </summary>
        public long? laboratory_id { get; set; }
        
        /// <summary>
        /// Identificador del establecimiento que realizo la toma de la muestra para el caso de sospecha.
        /// </summary>
        public long? establishment_id { get; set; }
        
        /// <summary>
        /// Identificador del usuario que creo el caso de sospecha. 
        /// </summary>
        public long? user_id { get; set; }
        
        /// <summary>
        /// Fecha en que se actualizó el caso de sospecha. 
        /// El formato de la fecha debe estar en el estándar ISO 8601
        /// </summary>
        public DateTime? updated_at { get; set; }
        
        /// <summary>
        /// Fecha en que se creó el caso de sospecha. 
        /// El formato de la fecha debe estar en el estándar ISO 8601
        /// </summary>
        public DateTime? created_at { get; set; }
        //public DateTime? deleted_at { get; set; }
        
        /// <summary>
        /// Indica si el caso es por búsqueda activa o por atención médica
        /// </summary>
        /// <remarks>
        /// true -> Búsqueda activa
        /// false -> Atención médica
        /// </remarks>
        public bool? busqueda_activa { get; set; }
    }
}
