using System;
using System.ComponentModel.DataAnnotations;

namespace WebService.Models
{
    public class SuspectCase
    {
        [Key]
        public long? id { get; set; }
        public int? age { get; set; }
        public string gender { get; set; }
        public DateTime? sample_at { get; set; }
        public int? epidemiological_week { get; set; }
        public string run_medic { get; set; }
        public bool? symptoms { get; set; }
        public DateTime? symptoms_at { get; set; }
        public DateTime? reception_at { get; set; }
        public long? receptor_id { get; set; }
        public DateTime? pcr_sars_cov_2_at { get; set; }
        public string pcr_sars_cov_2 { get; set; }
        public string sample_type { get; set; }
        public long? validator_id { get; set; }
        public string epivigila { get; set; }
        public bool? gestation { get; set; }
        public int? gestation_week { get; set; }
        public bool? close_contact { get; set; }
        public bool? functionary { get; set; }
        public string observation { get; set; }
        public long? patient_id { get; set; }
        public long? laboratory_id { get; set; }
        public long? establishment_id { get; set; }
        public long? user_id { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? created_at { get; set; }
        public long? minsal_ws_id { get; set; }
        public string ws_minsal_message { get; set; }
        public bool? ws_pntm_mass_sending { get; set; }
        public string case_type { get; set; }
    }
}
