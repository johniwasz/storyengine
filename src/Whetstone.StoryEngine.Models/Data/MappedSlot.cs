using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text;

namespace Whetstone.StoryEngine.Models.Data
{

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class MappedSlot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DataSlotType SlotType { get; set; }


        public string Alias { get; set; }

        public string Value { get; set; }

        [NotMapped]
        public string SlotTypeName { get; set; }


        private string DebuggerDisplay
        {

            get
            {
                StringBuilder sb = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(Alias))
                    sb.Append(Alias);

                sb.Append(':');

                if (!string.IsNullOrWhiteSpace(SlotTypeName))
                    sb.Append(SlotTypeName);

                sb.Append(':');

                if (SlotType != null)
                {
                    if (!string.IsNullOrWhiteSpace(SlotType.Name))
                        sb.Append(SlotType.Name);
                }


                sb.Append(':');


                if (!string.IsNullOrWhiteSpace(Value))
                    sb.Append(Value);


                return sb.ToString();
            }

        }
    }
}
