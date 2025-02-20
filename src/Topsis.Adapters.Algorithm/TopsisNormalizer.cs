﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using Topsis.Application.Contracts.Algorithm;
using Topsis.Domain;
using Topsis.Domain.Common;

namespace Topsis.Adapters.Algorithm
{
    internal class TopsisNormalizer
    {
        public DataTable Normalize(Workspace workspace, IList<StakeholderAnswerDto> answers)
        {
            var criteriaWeights = BuildWeights(workspace.Questionnaire.GetSettings(), answers);
            return BuildTable(criteriaWeights, answers, workspace.Questionnaire.AlternativesDictionary);
        }

        #region [ Helpers ]
        private IDictionary<int, double> BuildWeights(QuestionnaireSettings settings, 
            IList<StakeholderAnswerDto> answers)
        {
            // stakeholder weight / max weight
            var result = new Dictionary<int, double>();
            foreach (var g in answers.GroupBy(x => x.CriterionId))
            {
                double stakeholderWeight = g.First().CriterionWeight;
                double weight = stakeholderWeight / settings.CriterionWeightMax;
                result[g.Key] = weight;
            }

            return result;
        }

        private DataTable BuildTable(IDictionary<int, double> criteriaWeights, 
            IList<StakeholderAnswerDto> answers, 
            IDictionary<int, Alternative> alternatives)
        {
            var result = new DataTable();

            // build columns.
            result.Columns.Add(ColumnHelper.GetAlternativeColumnName(), typeof(int));
            result.Columns.Add(ColumnHelper.GetAlternativeTitleColumnName(), typeof(string));
            foreach (var c in criteriaWeights)
            {
                result.Columns.Add(ColumnHelper.GetCriterionColumnName(c.Key), typeof(double));
            }

            // build alternative rows.
            foreach (var item in answers.GroupBy(x => x.AlternativeId))
            {
                var criteria = item.ToArray();

                var dataRow = result.NewRow();
                dataRow[ColumnHelper.GetAlternativeColumnName()] = item.Key;
                if (alternatives.TryGetValue(item.Key, out var alt))
                {
                    dataRow[ColumnHelper.GetAlternativeTitleColumnName()] = alt.Title;
                }

                foreach (var c in criteria)
                {
                    var weight = criteriaWeights[c.CriterionId];
                    dataRow[ColumnHelper.GetCriterionColumnName(c.CriterionId)] = Rounder.Round(weight * c.AnswerValue);
                }

                result.Rows.Add(dataRow);
            }

            return result;
        }
        #endregion
    }
}
