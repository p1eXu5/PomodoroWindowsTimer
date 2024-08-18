using System;
using System.Data;
using System.Text;
using DbUp.Engine;
using PomodoroWindowsTimer.Types;

namespace PomodoroWindowsTimer.Migrator.sql;

public sealed class Script005_FillActiveTimePointId : IScript
{
    private static Dictionary<string, TimePoint> DefaultTimePoints;

    private static DbActiveTimePoint.DummyDbActiveTimePoint DummyDbActiveTimePoint { get; } = new();

    private DbActiveTimePoint.NewDbActiveTimePoint? _lastActiveTimePoint = null;

    static Script005_FillActiveTimePointId()
    {
        DefaultTimePoints = new(TimePointModule.Defaults.Length);

        foreach (var tp in TimePointModule.Defaults)
        {
            DefaultTimePoints[tp.Name] = tp;
        }
    }

    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        var cmd = dbCommandFactory();
        cmd.CommandText = """
            SELECT
                  we.id
                , we.event_json

            FROM work_event we
            WHERE
                event_json LIKE '%"Case":_"WorkStarted"%'
                OR
                event_json LIKE '%"Case":"WorkStarted"%'
                OR
                event_json LIKE '%"Case":_"BreakStarted"%'
                OR
                event_json LIKE '%"Case":"BreakStarted"%'
            ORDER BY created_at ASC
            """;

        var atpInsertScript = new StringBuilder("INSERT INTO active_time_point").AppendLine().AppendLine("VALUES");
        var workEventUpdateScript = new StringBuilder();

        string newWorkEventJson;
        const string updateWorkEventFormat = "UPDATE work_event SET event_json = '{1}', active_time_point_id = '{2}' WHERE id = '{0}';";

        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var id = reader.GetInt64(0);
                var event_json = reader.GetString(1);

                var workEventJson = JsonHelpers.Deserialize<WorkEventJson>(event_json);
                var atp = GetActiveTimePoint(workEventJson.Fields.Item2);

                switch (atp)
                {
                    case DbActiveTimePoint.NewDbActiveTimePoint newAtp:
                        atpInsertScript.AppendLine(newAtp.ToString());

                        newWorkEventJson =
                            JsonHelpers.SerializeNoIndent(
                                String.Equals(workEventJson.Case, "WorkStarted", StringComparison.Ordinal)
                                    ? WorkEvent.NewWorkStarted(workEventJson.Fields.Item1, workEventJson.Fields.Item2, newAtp.Value.Id)
                                    : WorkEvent.NewBreakStarted(workEventJson.Fields.Item1, workEventJson.Fields.Item2, newAtp.Value.Id)
                            );

                        workEventUpdateScript.AppendFormat(updateWorkEventFormat, id, newWorkEventJson, newAtp.Value.Id).AppendLine();
                        break;

                    case DbActiveTimePoint.DummyDbActiveTimePoint dummyAtp:
                        newWorkEventJson =
                            JsonHelpers.SerializeNoIndent(
                                String.Equals(workEventJson.Case, "WorkStarted", StringComparison.Ordinal)
                                    ? WorkEvent.NewWorkStarted(workEventJson.Fields.Item1, workEventJson.Fields.Item2, dummyAtp.Id)
                                    : WorkEvent.NewBreakStarted(workEventJson.Fields.Item1, workEventJson.Fields.Item2, dummyAtp.Id)
                            );

                        workEventUpdateScript.AppendFormat(updateWorkEventFormat, id, newWorkEventJson, dummyAtp.Id).AppendLine();
                        break;

                    case DbActiveTimePoint.ExistingDbActiveTimePoint existingAtp:
                        newWorkEventJson =
                            JsonHelpers.SerializeNoIndent(
                                String.Equals(workEventJson.Case, "WorkStarted", StringComparison.Ordinal)
                                    ? WorkEvent.NewWorkStarted(workEventJson.Fields.Item1, workEventJson.Fields.Item2, existingAtp.Id)
                                    : WorkEvent.NewBreakStarted(workEventJson.Fields.Item1, workEventJson.Fields.Item2, existingAtp.Id)
                            );

                        workEventUpdateScript.AppendFormat(updateWorkEventFormat, id, newWorkEventJson, existingAtp.Id).AppendLine();
                        break;
                }
            }
        }

        if (workEventUpdateScript.Length == 0)
        {
            return "";
        }

        string sql = atpInsertScript.Remove(atpInsertScript.Length - 3, 1).AppendLine(";").AppendLine().AppendLine(workEventUpdateScript.ToString()).ToString();

        return sql;
    }

    private DbActiveTimePoint GetActiveTimePoint(string timePointName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(timePointName));

        if (String.Equals(timePointName, _lastActiveTimePoint?.Value.Name, StringComparison.Ordinal))
        {
            return new DbActiveTimePoint.ExistingDbActiveTimePoint(_lastActiveTimePoint!.Value.Id);
        }

        if (DefaultTimePoints.TryGetValue(timePointName, out var tp))
        {
            return _lastActiveTimePoint =
                new DbActiveTimePoint.NewDbActiveTimePoint(
                    TimePointModule.ToActiveTimePoint(tp)
                );
        }

        return DummyDbActiveTimePoint;
    }

    private abstract record DbActiveTimePoint()
    {
        public sealed record NewDbActiveTimePoint(ActiveTimePoint Value) : DbActiveTimePoint
        {
            public override string ToString()
            {
                return $"    ('{Value.Id}', '{Value.OriginalId}', '{Value.Name}', '{new DateTime(Value.TimeSpan.Ticks).ToString("yyyy-MM-dd HH:mm:ss.fff")}', '{Value.Kind}', '{Value.KindAlias}', {TimeProvider.System.GetUtcNow().ToUnixTimeMilliseconds()}),";
            }
        }

        public sealed record DummyDbActiveTimePoint() : DbActiveTimePoint
        {
            public Guid Id { get; } = Guid.Empty;
        }

        public sealed record ExistingDbActiveTimePoint(Guid Id) : DbActiveTimePoint;
    }

    private record WorkEventJson(string Case, Tuple<DateTimeOffset, string> Fields);
}
