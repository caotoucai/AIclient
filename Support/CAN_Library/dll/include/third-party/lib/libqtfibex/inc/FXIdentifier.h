#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

/**
 * @brief element IDENTIFIER
 *
 * To describe a frames identifier on the communication system, usualy with a fixed identifierValue.
 */
class ASAM_FIBEX_EXPORT FXIdentifier
{
public:
    FXIdentifier();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element IDENTIFIER-VALUE */
    qint64 m_identifierValue;
    /** @todo element MANUFACTURER-EXTENSION */
};

} // FIBEX
} // ASAM
