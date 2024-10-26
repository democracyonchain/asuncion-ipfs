﻿using System.Collections.ObjectModel;
using System.Text.Json;

namespace AsuncionIpfs.Models
{
    /// <summary>
    /// A collection of <see cref="string" />
    /// </summary>
    public partial class StringCollection : Collection<string>
    {
        /// <summary>
        ///     Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            return ToJson();
        }

        /// <summary>
        ///     Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson(JsonSerializerOptions options = null)
        {
            return JsonSerializer.Serialize(this, options);
        }
    }
}
