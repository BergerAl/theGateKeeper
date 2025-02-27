import React, { useEffect } from 'react';

export const SvgLoader: React.FC<{ SvgComponent: React.FunctionComponent<React.SVGProps<SVGSVGElement>>, id: string }> = ({ SvgComponent, id }) => {
    useEffect(() => {
        const handleFocus = (event: FocusEvent) => {
            //@ts-ignore
            if (event.target?.id) {
                //@ts-ignore
                setRectangleBehindText(document.getElementById(id), event.target?.id)
            }
        };

        const handleFocusOut = (event: FocusEvent) => {
            //@ts-ignore
            if (event.target?.id) {
                //@ts-ignore
                removeRectangleBehindText(document.getElementById(id), event.target?.id)
            }
        };

        document.addEventListener('focusin', handleFocus);
        document.addEventListener('focusout', handleFocusOut);
        return () => {
            document.removeEventListener('focusin', handleFocus);
            document.removeEventListener('focusout', handleFocusOut);
        };
    }, [id]);
    return (
        <SvgComponent id={id} />
    )
};

function setRectangleBehindText(element: any, targetId: string) {
    var textElm = element.getElementById(targetId)
    if (textElm !== null) {
        var SVGRect = textElm.getBBox();
        var rect = document.createElementNS("http://www.w3.org/2000/svg", "rect");
        rect.setAttribute("id", `${targetId}-rect`)
        rect.setAttribute("x", SVGRect.x);
        rect.setAttribute("y", SVGRect.y);
        rect.setAttribute("width", SVGRect.width);
        rect.setAttribute("height", SVGRect.height);
        rect.setAttribute("fill", "#add8e6");
        element.insertBefore(rect, textElm);
    }
}

function removeRectangleBehindText(element: Document, targetId: string) {
    element.getElementById(`${targetId}-rect`)?.remove()
}