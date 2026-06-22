import React from 'react';
import { Clock, Droplets } from 'lucide-react';
import style from './ScheduleItem.module.css';

function ScheduleItem({ time, zones, duration, durationColor, repeats }) {

  const badgeStyle = {
    backgroundColor: `${durationColor}15`,
    border: `1px solid ${durationColor}50`,
    color: durationColor
  };

  return (
    <div className={style.ScheduleCard}>
      <div className="d-flex justify-content-between align-items-start w-100">
        <div className="d-flex flex-column gap-2">
          <div className="d-flex align-items-center gap-2">
            <Clock size={18} className="text-secondary" />
            <span className={style.TimeText}>{time}</span>
          </div>

          <div className="d-flex align-items-center gap-2">
            <Droplets size={16} className={style.ZoneIcon} />
            <span className={style.ZoneText}>{zones}</span>
          </div>

          <p className={style.RepeatText}>Repeats: {repeats}</p>
        </div>

        <div className={style.DurationBadge} style={badgeStyle}>
          {duration} min
        </div>
      </div>
    </div>
  );
}

export default ScheduleItem;