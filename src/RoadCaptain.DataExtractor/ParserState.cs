namespace RoadCaptain.DataExtractor
{
    public enum ParserState { Seeking, ReadingLiteral, ReadingToken, ReadingCoordinate,
        ReadingHeader,
        ReadingPath,
        ReadingEntry,
        SeekingToTable,
        ReadingTable
    }
}