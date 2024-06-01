-- SQL do mierzenia procentu good count
SELECT
    DeviceName,
    System.Timestamp() as windowEndTime,
    SUM(GoodCount) * 100 / (SUM(GoodCount) + SUM(BadCount)) as "good production percentage"
INTO
    [bloba]
FROM
    [maciek]
GROUP BY TumblingWindow(minute, 5), DeviceName
;


-- SQL do mierzenia temperatury (min, max, œrednia)
SELECT
    DeviceName,
    System.Timestamp() as windowEndTime,
    MAX(Temperature) as MaxTemperature,
    MIN(Temperature) as MinTemperature,
    AVG(Temperature) as AvgTemperature
INTO
    [temperature-calculations-blob]
FROM
    [maciek]
GROUP BY
    HoppingWindow(minute, 5, 1), DeviceName
;


--  SQL do sprawdzania b³êdów urz¹dzeñ
SELECT
    DeviceName,
    System.Timestamp() as windowEndTime,
    COUNT(*) as number_of_errors
INTO
    [device-errors-calculations-blob]
FROM
    [maciek]
WHERE
    DeviceError IS NOT null and DeviceError != 0
GROUP BY
    SlidingWindow(minute, 1), DeviceName
HAVING
    COUNT(DeviceError) > 3
;