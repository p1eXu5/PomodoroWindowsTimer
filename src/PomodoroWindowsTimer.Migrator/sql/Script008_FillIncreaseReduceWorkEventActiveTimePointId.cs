using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbUp.Engine;
using Microsoft.FSharp.Core;
using PomodoroWindowsTimer.Types;

namespace PomodoroWindowsTimer.Migrator.sql;
internal class Script008_FillIncreaseReduceWorkEventActiveTimePointId : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        var cmd = dbCommandFactory();
        cmd.CommandText = """
            SELECT
                  we.id
                , we.event_json

            FROM work_event we
            WHERE
                event_name IN ('WorkIncreased', 'WorkReduced', 'BreakIncreased', 'BreakReduced')
            ORDER BY created_at ASC
            ;
            """;

        var workEventUpdateScript = new StringBuilder();
        const string updateWorkEventFormat = "UPDATE work_event SET event_json = '{1}' WHERE id = '{0}';";

        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var id = reader.GetInt64(0);
                var event_json = reader.GetString(1);

                var workEventJson = JsonHelpers.Deserialize<WorkEventJson>(event_json);
                var newWorkEvent =
                    workEventJson.Case switch
                    {
                        "WorkIncreased" => WorkEvent.NewWorkIncreased(workEventJson.Fields.Item1, workEventJson.Fields.Item2, FSharpOption<Guid>.None),
                        "WorkReduced" => WorkEvent.NewWorkReduced(workEventJson.Fields.Item1, workEventJson.Fields.Item2, FSharpOption<Guid>.None),
                        "BreakIncreased" => WorkEvent.NewBreakIncreased(workEventJson.Fields.Item1, workEventJson.Fields.Item2, FSharpOption<Guid>.None),
                        "BreakReduced" => WorkEvent.NewBreakReduced(workEventJson.Fields.Item1, workEventJson.Fields.Item2, FSharpOption<Guid>.None),
                        _ => throw new ArgumentOutOfRangeException(nameof(workEventJson.Case), workEventJson.Case, null)
                    };

                workEventUpdateScript.AppendFormat(updateWorkEventFormat, id, JsonHelpers.SerializeNoIndent(newWorkEvent)).AppendLine();
            }
        }

        return workEventUpdateScript.ToString();
    }

    private record WorkEventJson(string Case, Tuple<DateTimeOffset, TimeSpan> Fields);
}