using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text;

namespace Whetstone.StoryEngine.Models.Data
{


    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class MappedIntent
    {
        public MappedIntent()
        {

        }

        public MappedIntent(DataIntent relatedIntent)
        {

            this.RelatedIntent = relatedIntent;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }



        [Column("ParentIntentId")]
        public DataIntent RelatedIntent { get; set; }

        [Column("Utterances")]
        public List<MappedUtterance> MappedUtterances { get; set; }


        private string DebuggerDisplay
        {
            get
            {

                string label = "nothing";
                if (RelatedIntent != null)
                    label = RelatedIntent.Name;

                if (MappedUtterances != null)
                {
                    label = string.Concat(label, ":", MappedUtterances.Count);

                }
                return label;

            }
        }
    }
}
