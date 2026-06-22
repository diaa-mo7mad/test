import style from"./Sensor.module.css"
function Sensor({name,value,unit,target,icon}){



    return(
    <div className={`${style.back} card`}>

    <div className={`d-flex  align-items-start justify-content-between  column-gap-5 ${style.space}`}>
        <div className="d-flex  align-items-start flex-column  justify-content-start  ">
            <h2 className={`${style.name}`}>{name}</h2>
            <div className="d-flex align-items-end">
                 <h3 className={`${style.value}`} >{value}</h3>
                 <span className={`${style.unit} mb-2 px-1`}  >{unit}</span>
            </div>
             <p className={`${style.target}`} >{target}</p>
        </div>
       <p className={` ${style.icon}  `}>{icon}</p>
    </div>

</div>


    )
}
export default Sensor