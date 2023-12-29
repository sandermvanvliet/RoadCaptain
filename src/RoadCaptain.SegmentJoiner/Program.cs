using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RoadCaptain;
using RoadCaptain.Adapters;

JsonSerializerSettings serializerSettings = new()
{
    ContractResolver = new CamelCasePropertyNamesContractResolver(),
    Converters =
    {
        new StringEnumConverter()
    }
};  

var segmentFileName = args[0];
var turnsFileName = args[1];
var segmentOneId = args[2];
var segmentTwoId = args[3];

var segments = new SegmentStore().LoadSegments(new World() { Id = "watopia" }, SportType.Both);
            
var turns = 
    JsonConvert.DeserializeObject<List<SegmentTurns>>(File.ReadAllText(turnsFileName), serializerSettings);

var segmentOne = segments.SingleOrDefault(s => s.Id == segmentOneId);

if (segmentOne == null)
{
    Console.Error.WriteLine($"Could not find segment {segmentOneId}");
    Environment.Exit(1);
}

var segmentTwo = segments.SingleOrDefault(s => s.Id == segmentTwoId);

if (segmentTwo == null)
{
    Console.Error.WriteLine($"Could not find segment {segmentTwoId}");
    Environment.Exit(1);
}

var combinedPoints = new List<TrackPoint>();
var nextSegmentsA = new List<Turn>();
var nextSegmentsB = new List<Turn>();

// Determine on which node the segments join
if (segmentOne.NextSegmentsNodeA.Any(turn => turn.SegmentId == segmentTwoId))
{
    if (segmentTwo.NextSegmentsNodeA.Any(turn => turn.SegmentId == segmentOneId))
    {
        // A joins to A 
        combinedPoints.AddRange(segmentOne.Points.OrderByDescending(point => point.Index));
        combinedPoints.AddRange(segmentTwo.Points.OrderBy(point => point.Index));

        nextSegmentsA = segmentOne.NextSegmentsNodeB;
        nextSegmentsB = segmentTwo.NextSegmentsNodeB;
    }
    else if (segmentTwo.NextSegmentsNodeB.Any(turn => turn.SegmentId == segmentOneId))
    {
        // A joins to B
        combinedPoints.AddRange(segmentOne.Points.OrderByDescending(point => point.Index));
        combinedPoints.AddRange(segmentTwo.Points.OrderByDescending(point => point.Index));

        nextSegmentsA = segmentOne.NextSegmentsNodeB;
        nextSegmentsB = segmentTwo.NextSegmentsNodeA;
    }
    else
    {
        Console.Error.WriteLine("No connection!");
        Environment.Exit(1);
    }
}
else if (segmentOne.NextSegmentsNodeB.Any(turn => turn.SegmentId == segmentTwo.Id))
{
    if (segmentTwo.NextSegmentsNodeA.Any(turn => turn.SegmentId == segmentOneId))
    {
        // B joins to A 
        // This is the easy one
        combinedPoints.AddRange(segmentOne.Points.OrderBy(point => point.Index));
        combinedPoints.AddRange(segmentTwo.Points.OrderBy(point => point.Index));

        nextSegmentsA = segmentOne.NextSegmentsNodeA;
        nextSegmentsB = segmentTwo.NextSegmentsNodeB;
    }
    else if (segmentTwo.NextSegmentsNodeB.Any(turn => turn.SegmentId == segmentOneId))
    {
        // B joins to B
        combinedPoints.AddRange(segmentOne.Points.OrderBy(point => point.Index));
        combinedPoints.AddRange(segmentTwo.Points.OrderByDescending(point => point.Index));

        nextSegmentsA = segmentOne.NextSegmentsNodeB;
        nextSegmentsB = segmentTwo.NextSegmentsNodeA;
    }
    else
    {
        Console.Error.WriteLine("No connection!");
        Environment.Exit(1);
    }
}
else
{
    Console.Error.WriteLine("What?");
    Environment.Exit(1);
}

segments.Remove(segmentOne);
segments.Remove(segmentTwo);

var combinedSegment = new Segment(combinedPoints)
{
    Id = segmentOne.Id,
    Name = segmentOne.Name,
    Sport = segmentOne.Sport,
    Type = segmentOne.Type,
    NoSelectReason = segmentOne.NoSelectReason
};

combinedSegment.NextSegmentsNodeA.AddRange(nextSegmentsA);
combinedSegment.NextSegmentsNodeB.AddRange(nextSegmentsB);

combinedSegment.CalculateDistances();

segments.Add(combinedSegment);

var turnsOne = turns.SingleOrDefault(t => t.SegmentId == segmentOneId);
var turnsTwo = turns.SingleOrDefault(t => t.SegmentId == segmentTwoId);

turns.Remove(turnsOne);
turns.Remove(turnsTwo);

var turnsA = new SegmentTurn();
var turnsB = new SegmentTurn();

if (combinedSegment.NextSegmentsNodeA.Count == 2)
{
    
}

turns.Add(new SegmentTurns
{
    SegmentId = combinedSegment.Id,
    TurnsA = turnsA,
    TurnsB = turnsB
});

File.WriteAllText(
    "split-segments.json",
    JsonConvert.SerializeObject(segments, Formatting.Indented, serializerSettings));

File.WriteAllText(
    "split-turns.json",
    JsonConvert.SerializeObject(turns, Formatting.Indented, serializerSettings));