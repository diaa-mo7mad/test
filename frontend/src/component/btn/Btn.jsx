import { Link } from "react-router-dom"
import style from "./Btn.module.css"
function Btn({regester}){
   
    if(regester==0)
    return(
        
        <div className={style.btnT}>
            <Link to={'/'} className={`${style.btn } ${style.activeBtn}`}>Sign In</Link>
            <Link to={'/Regester'} className={`${style.btn } `}>Register</Link>


        </div>

    )
    else {
          return(
        
        <div className={style.btnT}>
            <Link to={'/'} className={`${style.btn } `}>Sign In</Link>
            <Link to={'/Regester'} className={`${style.btn } ${style.activeBtn}`}>Register</Link>


        </div>

    )
    }

}
export default Btn