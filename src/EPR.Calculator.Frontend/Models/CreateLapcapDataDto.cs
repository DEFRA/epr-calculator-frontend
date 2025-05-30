﻿using EPR.Calculator.Frontend.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    /// <summary>
    /// Data Transfer Object (DTO) for creating lapcap parameter settings.
    /// </summary>
    public record CreateLapcapDataDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateLapcapDataDto"/> class.
        /// </summary>
        [SetsRequiredMembers]
        public CreateLapcapDataDto(LapcapRefreshViewModel original, string parameterYear)
        {
            this.LapcapDataTemplateValues = original.LapcapTemplateValue;
            this.LapcapFileName = original.FileName;
            this.ParameterYear = parameterYear;
        }

        /// <summary>
        /// Gets or sets the parameter year.
        /// </summary>
        /// <value>
        /// The year for which the parameters are set.
        /// </value>
        public required string ParameterYear { get; set; }

        /// <summary>
        /// Gets or sets the collection of lapcap parameter template values.
        /// </summary>
        /// <value>
        /// A collection of <see cref="LapcapDataTemplateValueDto"/> objects representing the template values for the lapcap parameters.
        /// </value>
        public required IEnumerable<LapcapDataTemplateValueDto> LapcapDataTemplateValues { get; set; }

        /// <summary>
        /// Gets or sets the parameter fielname.
        /// </summary>
        /// <value>
        /// The filename.
        /// </value>
        public required string LapcapFileName { get; set; }
    }
}