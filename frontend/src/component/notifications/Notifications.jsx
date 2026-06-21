import style from "./Notifications.module.css"

function Notifications({icon,header,content,time}){

   



    return(
        <div>
            <div className={`d-flex align-items-start justify-content-start ${style.back}`}>

                <div >
                <span className={`${style.contnet} my-3 px-3 d-block`}>{icon}</span>
                </div>

            <div className="d-flex  align-items-start justify-content-start flex-column my-3 ">
                <h3 className={`${style.contnet} ${style.header}`}>{header}</h3>
                <p className={`${style.contnet}  ${style.paragraph} `}>{content}</p>
                <span className={`${style.contnet}  ${style.time} `}>{time}</span>
            </div>


            </div>
            




        </div>
    )
}
export default Notifications;