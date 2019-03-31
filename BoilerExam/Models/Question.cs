using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations.Schema;
using BoilerExam.Models;
using System.ComponentModel.DataAnnotations;

using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;


namespace BoilerExam.Models
{
    public class Question
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }
        public Question Parent { get; set; }

        [Required]
        public string Content { get; set; }

        /**
         * JSON String indicating the available options and combinations
         * 
         * {
         *   "options": [
         *     "The content of Option A",
         *     "The content of Option B",
         *     "The content of Option C"
         *   ],
         *   "combinations": [ // Array of Indexes in options
         *     [0], // A
         *     [1], // B
         *     [2], // C
         *     [0, 1], // A and B
         *     [1, 2], // B and C
         *     [0, 1, 2] // All of them above
         *   ]
         * }
         * 
         */
        private string _OptionStr = "";

        [JsonIgnore]
        [Column("Options")]
        [Required]
        public string OptionStr {
            get
            {
                return _OptionStr;
            }
            set
            {
                _OptionStr = value ?? "[]";
                _Options = null;
            }
        }

        private string _CombinationStr = "";

        [JsonIgnore]
        [Column("Combinations")]
        public string CombinationStr {
            get
            {
                return _CombinationStr;
            }
            set
            {
                _CombinationStr = value;
                _Combinations = null;
            }
        }

        public int Answer { get; set; }

        private ICollection<string> _Options = null;

        [NotMapped]
        public ICollection<string> Options
        {
            set
            {
                OptionStr = value == null ? null : JsonConvert.SerializeObject(value);
                _Options = value;
            }
            get
            {
                if (_Options == null)
                    _Options = JsonConvert.DeserializeObject<ICollection<string>>(OptionStr);
                return _Options;
            }
        }

        private ICollection<IEnumerable<int>> _Combinations = null;

        [NotMapped]
        public ICollection<IEnumerable<int>> Combinations
        {
            set
            {
                CombinationStr = (value == null ? null : JsonConvert.SerializeObject(value));
                _Combinations = value;
            }
            get
            {
                if (_Combinations == null)
                    _Combinations = CombinationStr == null ? null : JsonConvert.DeserializeObject<ICollection<IEnumerable<int>>>(CombinationStr);
                return _Combinations;
            }
        }

        [JsonIgnore]
        public virtual ICollection<QuestionTag> QuestionTags { get; set; }

        [NotMapped]
        public IEnumerable<Tag> Tags
        {
            get => QuestionTags == null ? new Tag[] { } : QuestionTags.Select(r => r.Tag);
            set
            {
                QuestionTags = value.Select(v => new QuestionTag()
                {
                    TagId = v.Id
                }).ToList();
            }
        }

        public bool Verify()
        {
            var optionCount = Options.Count;
            if (Combinations == null)
            {
                if (Answer < 0 || Answer >= optionCount)
                    return false;
            }
            else
            {
                var combinationCount = Combinations.Count;
                if (Answer < 0 || Answer >= combinationCount)
                    return false;
                foreach (IEnumerable<int> optionIndexes in Combinations)
                {
                    foreach (int index in optionIndexes)
                    {
                        if (index < 0 || index >= optionCount)
                            return false;
                    }
                }
            }

            return true;
        }
        
    [ForeignKey("QuestionId")]
    public virtual ICollection<File> Attachments { get; set; }
  }
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [JsonIgnore]
        public virtual ICollection<QuestionTag> QuestionTags { get; set; }
    }
    public class QuestionTag
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        public virtual Tag Tag { get; set; }
        
        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }
    }
}
