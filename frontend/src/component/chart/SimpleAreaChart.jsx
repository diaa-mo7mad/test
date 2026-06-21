import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip,ResponsiveContainer } from 'recharts';
import style from "./SimpleAreaChart.module.css"
// #region Sample data


// #endregion
const SimpleAreaChart = ({data}) => {
  return (
 <div className={style.chart}>
      
      {/* 2. RESPONSIVE CONTAINER: Just fills the parent div completely */}
      <ResponsiveContainer width="100%" height="100%">
        
        {/* 3. CHART: No width/height/className props here */}
        <AreaChart
        className={`${style.Innerchart}`}
          data={data}
          margin={{ top: 20, right: 10, left: -20, bottom: 0 }}
        >
          <CartesianGrid strokeDasharray="10 10" />
          <XAxis dataKey="name" />
          <YAxis domain={[0, 100]} />
          <YAxis tickCount={5} />
          <Tooltip />
          <Area type="monotone" dataKey="uv" stroke="#8884d8" fill="#8884d8" />
        </AreaChart>

      </ResponsiveContainer>
    </div>
  );
};

export default SimpleAreaChart;