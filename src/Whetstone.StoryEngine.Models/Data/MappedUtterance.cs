using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text;

namespace Whetstone.StoryEngine.Models.Data
{ 
    [DebuggerDisplay("{" + nameof(Text) + "}")]
    public class MappedUtterance
    {

        public MappedUtterance()
        {

        }

        public MappedUtterance(string text)
        {
            this.Text = text;
        }

        public MappedUtterance(string text, List<MappedSlot> mappedSlots)
        {
            this.Text = text;
            this.MappedSlots = mappedSlots;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string Text { get; set; }



        public List<MappedSlot> MappedSlots { get; set; }

    }
}
