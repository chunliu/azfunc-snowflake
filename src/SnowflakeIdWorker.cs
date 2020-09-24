using System;

namespace chunliu.demo
{
    public class SnowflakeIdWorker
    {
        // Anchor timestamp (2015-01-01)
        private const long twepoch = 1420041600000L;
        // Bits for worker id
        private const int workerIdBits = 5;
        // Bits for data center id
        private const int datacenterIdBits = 5;
        // Max id of worker, 31
        private long maxWorkerId = -1 ^ (-1 << workerIdBits);
        // Max id of data center, 31
        private long maxDatacenterId = -1 ^ (-1 << datacenterIdBits);
        // Bits for the sequence number
        private const int sequenceBits = 12;
        // The position of work id
        private int workerIdShift = sequenceBits;
        // The position of data center id
        private int datacenterIdShift = sequenceBits + workerIdBits;
        // The position of the time stamp
        private int timestampLeftShift = sequenceBits + workerIdBits + datacenterIdBits;
        // The mask of the sequence (4095 (0b111111111111=0xfff=4095))
        private int sequenceMask = -1 ^ (-1 << sequenceBits);
        private long workerId;
        private long datacenterId;
        private long sequence = 0;
        private long lastTimestamp = -1;

        public SnowflakeIdWorker(int workerId, int datacenterId)
        {
            if (workerId > maxWorkerId || workerId < 0)
            {
                throw new ArgumentException($"Worker Id can't be greater than {maxWorkerId} or less than 0");
            }
            if (datacenterId > maxDatacenterId || datacenterId < 0)
            {
                throw new ArgumentException($"Data center Id can't be greater than {datacenterId} or less than 0");
            }
            this.workerId = workerId;
            this.datacenterId = datacenterId;
        }

        public long NextId()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (timestamp < lastTimestamp)
            {
                throw new Exception($"Clock moved backwards. Refusing to generate id for {lastTimestamp} milliseconds");
            }

            if (lastTimestamp == timestamp)
            {
                // If it's the same time, generate sequence within a millis
                sequence = (sequence + 1) & sequenceMask;
                if (sequence == 0) 
                {
                    // If the sequence is over the max, wait till next millis
                    timestamp = tillNextMillis(lastTimestamp);
                }
            }
            else
            {
                // Reset the sequence for the new time stamp
                sequence = 0;
            }
            lastTimestamp = timestamp;

            return ((timestamp - twepoch) << timestampLeftShift)
                | (datacenterId << datacenterIdShift)
                | (workerId << workerIdShift)
                | sequence;
        }

        protected long tillNextMillis(long timestamp)
        {
            long nextTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while (nextTimestamp <= timestamp)
            {
                nextTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
            return nextTimestamp;
        }
    }
}